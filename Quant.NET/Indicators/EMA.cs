using System.Runtime.CompilerServices;

namespace Quant.NET.Indicators;

/// <summary>
/// Exponential moving average
/// </summary>
public sealed class EMA
{
    private int _windowSize;
    private double _alpha;
    private double _average;
    private bool _isInitialized;

    public EMA(int windowSize)
    {
        _windowSize = windowSize;
        _alpha = 2.0 / (_windowSize + 1);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double AddValue(double x)
    {
        if (!_isInitialized)
        {
            _average = x;
            _isInitialized = true;
            return x;
        }

        _average = Math.FusedMultiplyAdd(x - _average, _alpha, _average);

        return _average;
    }

    public double? GetValue()
    {
        return _average;
    }
}