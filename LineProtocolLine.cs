using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zfs_collector;
internal class LineProtocolLine
{
    private string _source_line;
    private Dictionary<string, string> _fields;
    private Dictionary<string, string> _tags;
    private string _measurement;
    private long? _timestamp;
    internal LineProtocolLine(string line, bool process_timestamps = false)
    {
        _source_line = line;
        _tags = new();
        _fields = new();
        _measurement = "";
        try
        {
            _measurement = _source_line.Split(',')[0];
            foreach (string tag in _source_line.Split(' ')[0].Split(',').Skip(1).ToList())
            {
                _tags.Add(tag.Split('=')[0], tag.Split('=')[1]);
            }
            foreach (string field in _source_line.Split(' ')[1].Split(',').ToList())
            {
                _fields.Add(field.Split('=')[0], field.Split('=')[1].TrimEnd('u'));
            }
        }
        catch
        {
            System.Console.Error.WriteLine($"Error parsing line: {_source_line}, skipping.");
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
                System.Console.Error.WriteLine($"Error parsing timestamp for {_measurement}, skipping.");
                _timestamp = null;
            }
        }
    }
    internal List<string> PrometheusLines()
    {
        List<string> strings = new();
        foreach (KeyValuePair<string, string> field in _fields)
        {
            string tagstring = "";
            string timestampstr = (_timestamp != null) ? " " + _timestamp.ToString() : "";
            foreach (KeyValuePair<string, string> tag in _tags)
            {
                tagstring += $"{tag.Key}=\"{tag.Value}\",";
            }
            strings.Add($"{_measurement}_{field.Key}{{{tagstring.TrimEnd(',')}}} {field.Value}{timestampstr}");
        }
        return strings;
    }
}
