using System.Diagnostics;
using System.Text;
using System.Threading;

namespace zfs_collector;

internal class Program
{
    private static string _zpool_path = "zpool_influxdb";
    private static int _timeout = 10000;

    public static int Main(string[] args)
    {
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _zpool_path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                RedirectStandardError = true
            }
        };

        StringBuilder output = new();
        StringBuilder error = new();
        using (AutoResetEvent outputWaitHandle = new(false))
        using (AutoResetEvent errorWaitHandle = new(false))
        {
            process.OutputDataReceived += (sender, e) => {
                if (e.Data == null)
                {
                    outputWaitHandle.Set();
                }
                else
                {
                    output.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    errorWaitHandle.Set();
                }
                else
                {
                    error.AppendLine(e.Data);
                }
            };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Failed to start " + _zpool_path + " : " + ex.Message);
                return 1;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (process.WaitForExit(_timeout) &&
                outputWaitHandle.WaitOne(_timeout) &&
                errorWaitHandle.WaitOne(_timeout))
            {
                foreach (string line in output.ToString().Split('\n'))
                {
                    if (line != "")
                    {
                        LineProtocolLine lpl = new(line, false);
                        foreach (string promline in lpl.PrometheusLines())
                        {
                            System.Console.WriteLine(promline);
                        }
                    }
                }
                if (error.Length > 0)
                {
                    System.Console.Error.WriteLine(error.ToString());
                }
                return 0;
            }
            else
            {
                System.Console.Error.WriteLine("{0} timed out", _zpool_path);
                return 1;
            }
        }
    }
}