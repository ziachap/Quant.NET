using System.Collections;
using System.Text;

namespace Quant.NET.DataFrame;

public class Row : IEnumerable<KeyValuePair<string, double>>
{
    private readonly OrderedDictionary<string, double> _columns;

    public event Action<string>? OnInitializeColumn;

    public Row(IEnumerable<string> columns)
    {
        _columns = columns.ToOrderedDictionary(x => x, _ => 0d);
    }

    protected Row(OrderedDictionary<string, double> columns)
    {
        _columns = columns;
    }

    public Row Clone() => new(_columns);

    public double this[string column]
    {
        get { return _columns[column]; }
        set
        {
            if (_columns.ContainsKey(column)) _columns[column] = value;
            else
            {
                OnInitializeColumn?.Invoke(column);
                _columns[column] = value;
            }
        }
    }

    internal void InitializeColumn(string column)
    {
        _columns[column] = 0d;
    }

    internal void RemoveColumn(string column)
    {
        _columns.Remove(column);
    }


    internal bool ContainsColumn(string column)
    {
        return _columns.ContainsKey(column);
    }


    public IEnumerator<KeyValuePair<string, double>> GetEnumerator()
    {
        return _columns.Select(x => new KeyValuePair<string, double>(x.Key, x.Value)).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        var paddings = new Dictionary<string, int>();

        foreach (var col in _columns.Keys)
        {
            var maxValuePadding = this[col].ToString("0.000").Length;
            var padding = Math.Max(maxValuePadding, col.Length);
            paddings[col] = padding + 2;
        }

        foreach (var column in _columns)
        {
            sb.Append(column.Key.PadRight(paddings[column.Key]) + " ");
        }

        sb.AppendLine();
        
        sb.Append(ToValueString(paddings));

        sb.AppendLine();

        return sb.ToString();
    }

    public string ToValueString(Dictionary<string, int> paddings)
    {
        var sb = new StringBuilder();

        foreach (var column in _columns)
        {
            sb.Append(column.Value.ToString("0.000").PadRight(paddings[column.Key]) + " ");
        }

        return sb.ToString();
    }
}