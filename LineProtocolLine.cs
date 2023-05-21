using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zfs_collector;
internal class LineProtocolLine
{
    private string _source_line;
    private List<string>? _fields;
    private List<string>? _tags;
    private string? _measurement;
    private long? _timestamp;
    internal LineProtocolLine(string line)
    {
        _source_line = line;
    }
    internal List<string> PrometheusLines(bool process_timestamps = false)
    {
        List<string> strings = new();
        try
        {
            _measurement = _source_line.Split(',')[0];
            _tags = _source_line.Split(' ')[0].Split(',').Skip(1).ToList();
            _fields = _source_line.Split(' ')[1].Split(',').ToList();
        }
        catch
        {
            System.Console.Error.WriteLine($"Error parsing line: {_source_line}, skipping.");
            return strings;
        }
        _timestamp = null;
        if (process_timestamps)
        {
            try
            {
                _timestamp = Convert.ToInt64(_source_line.Split(' ')[2]) / 1000;
            }
            catch
            {
                System.Console.Error.WriteLine($"Error parsing timestamp for {_measurement} : \"{_timestamp.ToString}\", skipping.");
                _timestamp = null;
            }
        }

        foreach (string field in _fields)
        {
            string tagstring = "";
            string fieldname, fieldvalue, timestampstr;
            try
            {
                fieldname = field.Split('=')[0];
                fieldvalue = field.Split('=')[1].TrimEnd('u');
                timestampstr = (_timestamp != null) ? " " + _timestamp.ToString() : "";
            }
            catch
            {
                System.Console.Error.WriteLine($"Error parsing field for {_measurement} : \"{field}\", skipping.");
                continue;
            }
            foreach (string tag in _tags)
            {
                try
                {
                    tagstring += $"{tag.Split('=')[0]}=\"{tag.Split('=')[1]}\",";

                }
                catch
                {
                    System.Console.Error.WriteLine ($"Error parsing tag for {_measurement}_{field} : \"{tag}\", skipping.");
                }
            }
            strings.Add($"{_measurement}_{fieldname}{{{tagstring.TrimEnd(',')}}} {fieldvalue}{timestampstr}");
        }
        return strings;
    }
}
