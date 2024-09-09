namespace Quant.NET.DataFrame;

public class SMA
{
    private readonly Queue<double> values;
    private readonly int period;
    private double sum;

    public SMA(int period)
    {
        if (period <= 0)
        {
            throw new ArgumentException("Period must be greater than 0.", nameof(period));
        }

        this.period = period;
        this.values = new Queue<double>(period);
    }

    public double AddValue(double value)
    {
        sum += value;
        values.Enqueue(value);

        if (values.Count > period)
        {
            sum -= values.Dequeue();
        }

        return sum / Math.Min(period, values.Count);
    }
}