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
    private Dictionary<string, string> _fields = new();
    private Dictionary<string, string> _tags = new();
    private string _measurement = "";
    private long? _timestamp = null;
    internal LineProtocolLine(string line, bool process_timestamps = false)
    {
        _source_line = line;
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
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"Error parsing line: {_source_line}, skipping:\n{ex.Message}");
            _tags.Clear();
            _fields.Clear();
        }
        if (process_timestamps)
        {
            if (_source_line.Count(c => c == ' ') > 1)
            {
                try
                {
                    _timestamp = Convert.ToInt64(_source_line.Split(' ')[2]) / 1000;
                }
                catch (Exception ex)
                {
                    System.Console.Error.WriteLine($"Error parsing timestamp for {_measurement}, skipping:\n{ex.Message}");
                }
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
