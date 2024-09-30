namespace Quant.NET.Backtest;

public class Trade
{
    public Trade(double price, double quantity, Side side)
    {
        if (quantity <= 0) throw new ArgumentException("quantity must be > 0");

        Price = price;
        Quantity = quantity;
        Side = side;
        Idx = 0;
        Profit = 0;
    }

    public double Price { get; }
    public double Quantity { get; }
    public Side Side { get; }
    public int Idx { get; set; }
    public double Profit { get; set; }

    public override string ToString() => $"{Price} | {Quantity} | {Enum.GetName(typeof(Side), Side)}";
}