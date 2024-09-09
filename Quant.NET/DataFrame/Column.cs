using System.Collections;

namespace Quant.NET.DataFrame;

public class Column : IEnumerable<double>
{
    private readonly double[] _values;

    public Column(double[] values)
    {
        _values = values;
    }
    
    public double this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }
    
    public Column Window(int lookback, int lookahead, Func<IEnumerable<double>, double> func)
    {
        var r = new double[_values.Length];

        for (int i = 0; i < r.Length; i++)
        {
            var start = Math.Max(i - lookback, 0);
            var end = Math.Min(i + lookahead, _values.Length - 1);
            var range = _values[start..end];
            var val = func(range);
            r[end] = val;
        }

        return r.ToColumn();
    }

    /// <summary>
    /// Shift the data right (> 0) or left (&lt; 0).
    /// </summary>
    /// <param name="count">
    /// A positive count will add elements to the start (shift right).
    /// A negative count will remove elements from the start (shift left).
    /// </param>
    /// <param name="newElementValue"></param>
    /// <returns></returns>
    public Column Shift(int count, double newElementValue = 0d)
    {
        if (count == 0) return this;

        // Shift right
        if (count > 0)
        {
            var newValues = Enumerable.Repeat(newElementValue, count).ToArray();

            var result = new double[newValues.Length + _values.Length];
            newValues.CopyTo(result, 0);
            _values.CopyTo(result, newValues.Length);

            return result.ToColumn();
        }

        // Shift left
        return _values.Skip(-count).ToColumn();
    }
    
    public Column Ema(int period)
    {
        var ema = new EMA(period);

        var r = _values.Select(x => ema.AddValue(x)).ToColumn();

        return r;
    }

    public Column Sma(int period)
    {
        var ema = new SMA(period);

        var r = _values.Select(x => ema.AddValue(x)).ToColumn();

        return r;
    }

    public double Volatility()
    {
        if (_values.Length == 0)
        {
            throw new InvalidOperationException("Cannot calculate volatility on an empty column.");
        }

        double average = _values.Average();
        double sumOfSquaresOfDifferences = _values.Select(val => (val - average) * (val - average)).Sum();
        double variance = sumOfSquaresOfDifferences / _values.Length;

        return Math.Sqrt(variance);
    }


    // Method to calculate the difference from the previous value
    public Column Diff()
    {
        if (_values.Length == 0)
        {
            throw new InvalidOperationException("Cannot calculate differences on an empty column.");
        }

        double[] differences = new double[_values.Length];
        differences[0] = 0; // First element has no previous value

        for (int i = 1; i < _values.Length; i++)
        {
            differences[i] = _values[i] - _values[i - 1];
        }

        return differences.ToColumn();
    }

    // Method to calculate the percentage difference from the previous value
    public Column DiffPct()
    {
        if (_values.Length == 0)
        {
            throw new InvalidOperationException("Cannot calculate percentage differences on an empty column.");
        }

        double[] percentageDifferences = new double[_values.Length];
        percentageDifferences[0] = 0; // First element has no previous value

        for (int i = 1; i < _values.Length; i++)
        {
            if (_values[i - 1] != 0)
            {
                percentageDifferences[i] = ((_values[i] - _values[i - 1]) / _values[i - 1]) * 100;
            }
            else
            {
                percentageDifferences[i] = 0; // Avoid division by zero
            }
        }

        return percentageDifferences.ToColumn();
    }

    #region OP_COLUMN_COLUMN

    public static Column operator +(Column a, Column b)
    {
        return PerformArithmetic(a, b, (x, y) => x + y);
    }

    public static Column operator *(Column a, Column b)
    {
        return PerformArithmetic(a, b, (x, y) => x * y);
    }

    public static Column operator /(Column a, Column b)
    {
        return PerformArithmetic(a, b, (x, y) => x / y);
    }

    public static Column operator -(Column a, Column b)
    {
        return PerformArithmetic(a, b, (x, y) => x - y);
    }

    private static Column PerformArithmetic(Column a, Column b, Func<double, double, double> func)
    {
        var length = Math.Min(a.Length, b.Length);
        var r = new double[length];

        for (int i = 0; i < length; i++)
        {
            var x = a[i];
            var y = b[i];
            r[i] = func(x, y);
        }

        return new Column(r);
    }

    #endregion
    
    #region OP_COLUMN_INT
    public static Column operator +(Column a, int b)
    {
        return PerformArithmetic(a, b, (x, y) => x + y);
    }

    public static Column operator *(Column a, int b)
    {
        return PerformArithmetic(a, b, (x, y) => x * y);
    }

    public static Column operator /(Column a, int b)
    {
        return PerformArithmetic(a, b, (x, y) => x / y);
    }

    public static Column operator -(Column a, int b)
    {
        return PerformArithmetic(a, b, (x, y) => x - y);
    }

    /// <summary>
    /// Returns 1 if true and 0 if false for each element
    /// </summary>
    public static Column operator >(Column a, int b)
    {
        return PerformArithmetic(a, b, (x, y) => x > y ? 1 : 0);
    }

    /// <summary>
    /// Returns 1 if true and 0 if false for each element
    /// </summary>
    public static Column operator <(Column a, int b)
    {
        return PerformArithmetic(a, b, (x, y) => x < y ? 1 : 0);
    }
    
    #endregion

    #region OP_COLUMN_DOUBLE
    public static Column operator +(Column a, double b)
    {
        return PerformArithmetic(a, b, (x, y) => x + y);
    }

    public static Column operator *(Column a, double b)
    {
        return PerformArithmetic(a, b, (x, y) => x * y);
    }

    public static Column operator /(Column a, double b)
    {
        return PerformArithmetic(a, b, (x, y) => x / y);
    }

    public static Column operator -(Column a, double b)
    {
        return PerformArithmetic(a, b, (x, y) => x - y);
    }

    private static Column PerformArithmetic(Column a, double b, Func<double, double, double> func)
    {
        var length = a.Length;
        var r = new double[length];

        for (int i = 0; i < length; i++)
        {
            var x = a[i];
            r[i] = func(x, b);
        }

        return new Column(r);
    }

    #endregion

    public IEnumerator<double> GetEnumerator()
    {
        return _values.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Length => _values.Length;
    
    public override string ToString()
    {
        return $"[{string.Join(", ", _values)}]";
    }
}