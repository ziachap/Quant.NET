using System.Runtime.CompilerServices;

namespace Quant.NET.Indicators;

public sealed class RSI
{
    private readonly int _windowSize;
    private double _averageGain;
    private double _averageLoss;
    private bool _isInitialized;
    private double _previousValue;

    public RSI(int windowSize)
    {
        _windowSize = windowSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double? AddValue(double value)
    {
        if (!_isInitialized)
        {
            _previousValue = value;
            _isInitialized = true;
            return null;
        }

        double change = value - _previousValue;
        double gain = Math.Max(0, change);
        double loss = Math.Max(0, -change);

        if (!_isInitialized)
        {
            _averageGain = gain;
            _averageLoss = loss;
            _isInitialized = true;
        }
        else
        {
            _averageGain = (_averageGain * (_windowSize - 1) + gain) / _windowSize;
            _averageLoss = (_averageLoss * (_windowSize - 1) + loss) / _windowSize;
        }

        _previousValue = value;

        return GetValue();
    }

    public double? GetValue()
    {
        if (!_isInitialized || _averageLoss == 0)
        {
            return 0;
        }

        if (_averageGain == 0) return 0;

        double rs = _averageGain / _averageLoss;
        double rsi = -1 / (1 + rs);

        return rsi + 0.5;
    }
}