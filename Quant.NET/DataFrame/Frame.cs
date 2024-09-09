using System.Collections;
using System.Text;

namespace Quant.NET.DataFrame
{
    public class Frame : IEnumerable<Row>
    {
        // TODO: Linked list

        private readonly List<Row> _rows;
        private readonly List<string> _schema;

        public int Count => _rows.Count;

        public Frame()
        {
            _rows = new List<Row>();
            _schema = new List<string>();
        }

        internal Frame(List<Row> rows, List<string> schema)
        {
            _rows = rows;
            _schema = schema;
        }

        // TODO
        public static Frame LoadCsv(string path)
        {
            var stream = File.ReadLines(path);

            if (!stream.Any()) throw new Exception("File is empty.");

            var headers = stream.First().Split(',', StringSplitOptions.TrimEntries);

            var firstValues = stream.Skip(1).First().Split(',', StringSplitOptions.TrimEntries);

            var parseableIndexes = new List<int>();
            for (int i = 0; i < firstValues.Length; i++)
            {
                if (double.TryParse(firstValues[i], out _))
                {
                    parseableIndexes.Add(i);
                }
            }

            var dateTimeIndexes = new List<int>();
            for (int i = 0; i < firstValues.Length; i++)
            {
                if (DateTime.TryParse(firstValues[i], out _))
                {
                    dateTimeIndexes.Add(i);
                }
            }

            int? dateTimeIdx = null;
            if (dateTimeIndexes.Count == 1)
            {
                dateTimeIdx = dateTimeIndexes.First();
            }

            var f = new Frame();

            foreach (var line in stream.Skip(1))
            {
                var values = line.Split(',', StringSplitOptions.TrimEntries);

                var row = f.CreateRow();

                if (dateTimeIdx.HasValue)
                {
                    var header = headers[dateTimeIdx.Value];
                    var value = DateTime.Parse(values[dateTimeIdx.Value]).DateTimeToUnixTimeStamp();

                    row[header] = value;
                }

                foreach (var idx in parseableIndexes)
                {
                    var header = headers[idx];
                    var value = double.Parse(values[idx]);

                    row[header] = value;
                }
            }

            return f;
        }

        public Frame Clone() => new(_rows, _schema);

        public Column this[string column]
        {
            get => new Column(_rows.Select(x => x[column]).ToArray());
            set
            {
                var length = value.Length;
                var i = 0;

                if (!_schema.Contains(column))
                {
                    _schema.Add(column);
                }

                if (_rows.Any())
                {
                    foreach (var row in _rows)
                    {
                        row.InitializeColumn(column);

                        if (i < length)
                        {
                            row[column] = value[i];
                        }

                        i++;
                    }
                }
                else
                {
                    foreach (var v in value)
                    {
                        var row = CreateRow();
                        row[column] = v;
                    }
                }
            }
        }

        public Frame this[Range range] => new(_rows[range], _schema);

        public Row this[int rowIdx] => _rows[rowIdx];

        public Row CreateRow()
        {
            var row = new Row(_schema);

            row.OnInitializeColumn += CreateColumn;

            _rows.Add(row);
            return row;
        }

        public void CreateColumn(string column)
        {
            _schema.Add(column);

            foreach (var row in _rows)
            {
                row.InitializeColumn(column);
            }
        }

        public Frame TakeLastWhile(Predicate<Row> predicate)
        {
            var rows = new List<Row>();

            foreach (var row in Enumerable.Reverse(_rows))
            {
                if (predicate(row))
                {
                    rows.Add(row);
                }
            }

            return new Frame(Enumerable.Reverse(rows).ToList(), _schema);
        }

        public Frame SkipWhile(Predicate<Row> predicate)
        {
            var rows = _rows.SkipWhile(row => predicate(row));
            return new Frame(rows.ToList(), _schema);
        }
        
        /// <summary>
        /// For each element, look forward until an element meets the condition, then select from that element.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public Column SeekForwardSelect(Func<Row, Row, bool> predicate, Func<Row, Row, double> selector)
        {
            var values = new List<double>();

            for (int i = 0; i < _rows.Count; i++)
            {
                var curr = _rows[i];

                for (int j = i; j < _rows.Count; j++)
                {
                    var iter = _rows[j];

                    if (predicate(curr, iter))
                    {
                        values.Add(selector(curr, iter));
                        break;
                    }
                }
            }

            return values.ToColumn();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!_rows.Any()) return "<empty>";

            var paddings = new Dictionary<string, int>();

            var lastRow = _rows.Last();
            foreach (var col in _schema)
            {
                var maxValuePadding = lastRow[col].ToString("0.000").Length;
                var padding = Math.Max(maxValuePadding, col.Length);
                paddings[col] = padding + 2;
            }
            
            foreach (var column in _schema)
            {
                sb.Append(column.PadRight(paddings[column]) + " ");
            }

            sb.AppendLine();

            if (_rows.Count < 15)
            {
                foreach (var row in _rows)
                {
                    sb.AppendLine(row.ToValueString(paddings));
                }
            }
            else
            {
                var first = _rows.Take(5);
                var last = _rows.TakeLast(5);

                foreach (var row in first)
                {
                    sb.AppendLine(row.ToValueString(paddings));
                }

                sb.AppendLine("...");

                foreach (var row in last)
                {
                    sb.AppendLine(row.ToValueString(paddings));
                }
            }

            sb.AppendLine(_rows.Count.ToString("N0") + " Rows");

            return sb.ToString();
        }

        public IEnumerator<Row> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
