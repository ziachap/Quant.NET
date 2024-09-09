namespace Quant.NET.DataFrame;

public class RollingEma
{
    private double _ema;
    private bool _isInitialized;
    private long _lastTimestamp;
    private readonly double _decayRate;

    public RollingEma(double decayRate)
    {
        if (decayRate <= 0 || decayRate >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(decayRate), "Decay rate must be between 0 and 1 (exclusive).");
        }

        _decayRate = decayRate;
        _isInitialized = false;
    }

    public double CurrentEma => _ema;

    public void AddEvent(long timestamp, double value)
    {
        if (!_isInitialized)
        {
            _ema = value;
            _lastTimestamp = timestamp;
            _isInitialized = true;
            return;
        }

        // Calculate time difference
        double deltaTime = (timestamp - _lastTimestamp) / 1000.0; // convert milliseconds to seconds
        _lastTimestamp = timestamp;

        // Adjust smoothing factor based on time difference
        double alpha = 1 - Math.Exp(-_decayRate * deltaTime);

        // Calculate the new EMA value
        _ema = alpha * value + (1 - alpha) * _ema;
    }
}