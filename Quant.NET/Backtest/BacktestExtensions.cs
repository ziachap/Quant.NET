using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quant.NET.DataFrame;

namespace Quant.NET.Backtest
{
    public static class BacktestExtensions
    {
        public static Frame Backtest(this Frame f, string strategyName, string priceColumn, Backtester.BacktestDelegate onTick)
        {
            var backtest = new Backtester(priceColumn, onTick);

            var fr = f.Clone();

            for (var i = 0; i < f.Count; i++)
            {
                var row = f[i];
                backtest.OnTick(row);

                var rr = fr[i];

                rr[$"{strategyName}_Equity"] = backtest.Equity;
                //rr[$"{strategyName}_Cash"] = backtest.Cash;
            }

            return fr;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="equitycolumn"></param>
        /// <param name="annualTradingPeriods">Number of data points in a year. Used to annualize the ratios.</param>
        /// <returns></returns>
        public static EquityMetrics AsEquityMetrics(this Column equitycolumn, int annualTradingPeriods = 252)
        {
            var returns = equitycolumn.DiffPct();

            var ev = returns.Average();

            var sharpeRatio = ev / returns.StandardDeviation();
            var sortinoRatio = ev / returns.Where(x => x < 0).ToColumn().StandardDeviation();

            sharpeRatio *= Math.Sqrt(annualTradingPeriods);
            sortinoRatio *= Math.Sqrt(annualTradingPeriods);

            return new EquityMetrics()
            {
                ExpectedValue = ev,
                SharpeRatio = sharpeRatio,
                SortinoRatio = sortinoRatio,
            };
        }
    }

    public class Backtester
    {
        public delegate void BacktestDelegate(Row x, Backtester engine);

        private readonly string _priceColumn;
        private readonly BacktestDelegate _onTick;

        public Backtester(string priceColumn, BacktestDelegate onTick)
        {
            _priceColumn = priceColumn;
            _onTick = onTick;
        }
        
        public Dictionary<string, double> State { get; } = new Dictionary<string, double>();
        public NetPosition Position { get; } = new NetPosition();
        public double Cash { get; private set; } = 100;
        public double Equity { get; private set; } = 100;

        private double _currentPrice;

        internal void OnTick(Row row)
        {
            _currentPrice = row[_priceColumn];

            _onTick(row, this);

            var unrealizedPnl = (_currentPrice - Position.AvgPrice) * Position.Quantity;
            Equity = Cash + unrealizedPnl;
        }

        public void SetPosition(double pos)
        {
            if (Position.Quantity == pos) return;

            var q = Math.Abs(pos - Position.Quantity);
            if (Position.Quantity < pos) Trade(new Trade(_currentPrice, q, Side.Buy));
            if (Position.Quantity > pos) Trade(new Trade(_currentPrice, q, Side.Sell));
        }

        public void Trade(Trade trade)
        {
            var pnl = Position.ConsolidateTrade(trade);

            Cash += pnl;
        }
    }

    public class EquityMetrics
    {
        public double CAGR { get; set; }
        public double SharpeRatio { get; set; }
        public double SortinoRatio { get; set; }
        public double ExpectedValue { get; set; }
        public double MaxDrawdown { get; set; }

        public void Print()
        {
            Console.WriteLine("EV:".PadLeft(18) + $" {ExpectedValue:F3}");
            Console.WriteLine("Sharpe:".PadLeft(18) + $" {SharpeRatio:F2}");
            Console.WriteLine("Sortino:".PadLeft(18) + $" {SortinoRatio:F2}");
        }
    }
}
