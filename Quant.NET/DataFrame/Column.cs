using System.Collections;
using Quant.NET.Indicators;
using Skender.Stock.Indicators;

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
            var range = _values[start..(end + 1)];
            var val = func(range);
            r[end] = val;
        }

        return r.ToColumn();
    }

    public Column HurstExponent(int period)
    {
        return _values.Select(x => new Quote()
        {
            Open = (decimal)x,
            High = (decimal)x,
            Low = (decimal)x,
            Close = (decimal)x,
        }).GetHurst(period).Select(x => x.HurstExponent ?? 0.5).ToColumn();
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

    public Column Crossover(double constant, bool crossAbove = true)
    {
        var crossoverValues = new double[Length];
        crossoverValues[0] = 0d;

        for (int i = 1; i < Length; i++)
        {
            if (crossAbove)
            {
                // Check if this column crosses above the constant
                crossoverValues[i] = (this[i - 1] <= constant && this[i] > constant) ? 1 : 0;
            }
            else
            {
                // Check if this column crosses below the constant
                crossoverValues[i] = (this[i - 1] >= constant && this[i] < constant) ? 1 : 0;
            }
        }

        return new Column(crossoverValues);
    }

    public Column ZScore(int period)
    {
        if (period <= 0 || period > _values.Length)
            throw new ArgumentException("Period must be greater than 0 and less than or equal to the number of values in the column.");
        
        return Window(period, 0, window =>
        {
            if (!window.Any()) return 0;

            var normaliser = 1d;
            if (window.Count() < period)
            {
                normaliser = Math.Sqrt(period - window.Count());
            }

            var mean = window.Average();
            var stdDev = Math.Sqrt(window.Sum(x => Math.Pow(x - mean, 2)) / period);
            return stdDev == 0 ? 0 : (window.Last() - mean) / (stdDev * normaliser);
        });

    }

    public Column Ema(int period)
    {
        var ema = new EMA(period);
        var r = _values.Select(x => ema.AddValue(x)).ToColumn();
        return r;
    }

    public Column Sma(int period)
    {
        var sma = new SMA(period);
        var r = _values.Select(x => sma.AddValue(x)).ToColumn();
        return r;
    }

    public Column Rsi(int period)
    {
        var rsi = new RSI(period);
        var r = _values.Select(x => rsi.AddValue(x) ?? 0).ToColumn();
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

    public double StandardDeviation()
    {
        if (_values.Length == 0)
            throw new InvalidOperationException("Standard deviation cannot be calculated for an empty collection.");

        var mean = _values.Average();
        var sumOfSquaresOfDifferences = _values.Select(val => Math.Pow(val - mean, 2)).Sum();
        var standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / _values.Length);

        return standardDeviation;
    }

    /// <summary>
    /// Calculate the difference from the previous value
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Calculate the percentage difference from the previous value
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
            if (_values[i-1] != 0)
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


    public Column CumSum()
    {
        var sum = 0d;
        var result = new List<double>();

        foreach (var value in _values)
        {
            sum += value;
            result.Add(sum);
        }

        return result.ToColumn();
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

    /// <summary>
    /// Returns 1 if true and 0 if false for each element
    /// </summary>
    public static Column operator >(Column a, Column b)
    {
        return PerformArithmetic(a, b, (x, y) => x > y ? 1 : 0);
    }

    /// <summary>
    /// Returns 1 if true and 0 if false for each element
    /// </summary>
    public static Column operator <(Column a, Column b)
    {
        return PerformArithmetic(a, b, (x, y) => x < y ? 1 : 0);
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
    
    // TODO: constant * Column (left-hand constant)
    
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