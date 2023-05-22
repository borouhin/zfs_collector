using System.Diagnostics;
using System.Text;

namespace zfs_collector;

internal class Program
{
    private const string _zpoolPath = "zpool_influxdb";
    private const int _timeout = 10000;

    public static int Main()
    {
        using AutoResetEvent outputWaitHandle = new(false);
        using AutoResetEvent errorWaitHandle = new(false);
        StringBuilder output = new();
        StringBuilder error = new();
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _zpoolPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                RedirectStandardError = true
            }
        };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                outputWaitHandle.Set();
            }
            else
            {
                output.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
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
            Console.Error.WriteLine($"Failed to start {_zpoolPath} : {ex.Message}");
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
                if (line == "") continue;
                LineProtocolLine lpl = new(line);
                foreach (string promline in lpl.PrometheusLines())
                {
                    Console.WriteLine(promline);
                }
            }
            if (error.Length <= 0) return 0;
            Console.Error.WriteLine($"Errors encountered running {_zpoolPath} :\n{error}");
            return 1;
        }
        Console.Error.WriteLine($"{_zpoolPath} timed out, no response in {_timeout / 1000} s.");
        return 1;
    }
}