namespace Quant.NET.Backtest;

public class NetPosition
{
    public double AvgPrice;
    public double Quantity;

    public override string ToString() => $"{AvgPrice} | {Quantity}";
    
    /// <summary>
    /// Consolidates the trade into the net position.
    /// </summary>
    /// <param name="trade"></param>
    /// <returns>Profit generated from the trade</returns>
    public double ConsolidateTrade(Trade trade)
    {
        if (Quantity == 0)
        {
            AvgPrice = trade.Price;
            Quantity = trade.Side == Side.Buy ? trade.Quantity : -trade.Quantity;
            return 0;
        }

        var netPrice = AvgPrice;
        var netQuantity = Quantity;
        var netQuantityAbs = Math.Abs(Quantity);

        var totalQuantity = netQuantityAbs + trade.Quantity;

        switch (trade.Side)
        {
            case Side.Buy when Quantity > 0:
                var avgPrice1 = ((AvgPrice * Quantity) + (trade.Price * trade.Quantity))
                                / totalQuantity;
                AvgPrice = avgPrice1;
                Quantity = totalQuantity;
                break;
            case Side.Sell when Quantity < 0:
                var avgPrice2 = ((AvgPrice * netQuantityAbs) + (trade.Price * trade.Quantity))
                                / totalQuantity;
                AvgPrice = avgPrice2;
                Quantity = -totalQuantity;
                break;
            case Side.Buy when Quantity < 0:
                var difference1 = netPrice - trade.Price;
                var profit1 = difference1 * Math.Min(trade.Quantity, netQuantityAbs);

                // Full close
                if (trade.Quantity == netQuantityAbs)
                {
                    AvgPrice = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (trade.Quantity <= netQuantityAbs)
                {
                    AvgPrice = netPrice;
                    Quantity = netQuantity + trade.Quantity;
                }
                // Full close and flip to long
                else
                {
                    AvgPrice = trade.Price;
                    Quantity += trade.Quantity;
                }

                trade.Profit = profit1;
                return profit1;
            case Side.Sell when Quantity > 0:
                var difference2 = trade.Price - netPrice;
                var profit2 = difference2 * Math.Min(trade.Quantity, netQuantityAbs);

                // Full close
                if (trade.Quantity == netQuantityAbs)
                {
                    AvgPrice = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (trade.Quantity < netQuantityAbs)
                {
                    AvgPrice = netPrice;
                    Quantity = netQuantity - trade.Quantity;
                }
                // Full close and flip to short
                else
                {
                    AvgPrice = trade.Price;
                    Quantity -= trade.Quantity;
                }

                trade.Profit = profit2;
                return profit2;
        }

        return 0;
    }

    /// <summary>
    /// Consolidates the trade into the net position.
    /// </summary>
    /// <returns>Profit generated from the trade</returns>
    public double ConsolidateTrade(Side side, long quantity, double price)
    {
        if (Quantity == 0)
        {
            AvgPrice = price;
            Quantity = side == Side.Buy ? quantity : -quantity;
            return 0;
        }

        var netPrice = AvgPrice;
        var netQuantity = Quantity;
        var netQuantityAbs = Math.Abs(Quantity);

        var totalQuantity = netQuantityAbs + quantity;

        switch (side)
        {
            case Side.Buy when Quantity > 0:
                var avgPrice1 = ((AvgPrice * Quantity) + (price * quantity))
                                / totalQuantity;
                AvgPrice = avgPrice1;
                Quantity = totalQuantity;
                break;
            case Side.Sell when Quantity < 0:
                var avgPrice2 = ((AvgPrice * netQuantityAbs) + (price * quantity))
                                / totalQuantity;
                AvgPrice = avgPrice2;
                Quantity = -totalQuantity;
                break;
            case Side.Buy when Quantity < 0:
                var difference1 = netPrice - price;
                var profit1 = difference1 * Math.Min(quantity, netQuantityAbs);

                // Full close
                if (quantity == netQuantityAbs)
                {
                    AvgPrice = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (quantity <= netQuantityAbs)
                {
                    AvgPrice = netPrice;
                    Quantity = netQuantity + quantity;
                }
                // Full close and flip to long
                else
                {
                    AvgPrice = price;
                    Quantity += quantity;
                }

                return profit1;
            case Side.Sell when Quantity > 0:
                var difference2 = price - netPrice;
                var profit2 = difference2 * Math.Min(quantity, netQuantityAbs);

                // Full close
                if (quantity == netQuantityAbs)
                {
                    AvgPrice = 0;
                    Quantity = 0;
                }
                // Partial close
                else if (quantity < netQuantityAbs)
                {
                    AvgPrice = netPrice;
                    Quantity = netQuantity - quantity;
                }
                // Full close and flip to short
                else
                {
                    AvgPrice = price;
                    Quantity -= quantity;
                }

                return profit2;
        }

        return 0;
    }

    public double CurrentPnl(double mktPrice)
    {
        return (mktPrice - AvgPrice) * Quantity;
    }
}