namespace zfs_collector;
internal class LineProtocolLine
{
    private readonly Dictionary<string, string> _fields = new();
    private readonly Dictionary<string, string> _tags = new();
    private readonly string _measurement = "";
    private readonly long? _timestamp = null;
    internal LineProtocolLine(string line, bool process_timestamps = false)
    {
        try
        {
            _measurement = line.Split(',')[0];
            foreach (string tag in line.Split(' ')[0].Split(',').Skip(1).ToList())
            {
                _tags.Add(tag.Split('=')[0], tag.Split('=')[1]);
            }
            foreach (string field in line.Split(' ')[1].Split(',').ToList())
            {
                _fields.Add(field.Split('=')[0], field.Split('=')[1].TrimEnd('u'));
            }
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"Error parsing line: {line}, skipping:\n{ex.Message}");
            _tags.Clear();
            _fields.Clear();
        }

        if (!process_timestamps) return;
        if (line.Count(c => c == ' ') <= 1) return;
        try
        {
            _timestamp = Convert.ToInt64(line.Split(' ')[2]) / 1000;
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"Error parsing timestamp for {_measurement}, skipping:\n{ex.Message}");
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
