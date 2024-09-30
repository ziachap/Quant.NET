using System.Runtime.InteropServices.JavaScript;
using Quant.NET.Backtest;
using Quant.NET.DataFrame;
using Quant.NET.Plots;

namespace Quant.NET
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========== Quant.NET ==========");
            Console.WriteLine("Demo app");

            var f = new Frame();

            f["Counter"] = Enumerable.Range(1, 100).Select(x => (double)x).ToColumn();
            f["Counter2"] = f["Counter"] * 2;
            f["Counter3"] = Enumerable.Range(1, 55).Select(x => (double)x).ToColumn();

            Console.WriteLine(f);

            var f2 = Frame.LoadCsv("C:\\data\\csv\\spx\\SPX.csv");
            var f3 = Frame.LoadCsv("C:\\data\\csv\\vix\\VIX_History.csv", "MM/dd/yyyy");

            f2["Return"] = f2["Close"].Diff();
            f2["ReturnPct"] = f2["Close"].DiffPct();
            f2["NextReturnPct"] = f2["Close"].Shift(-1).DiffPct();

            Console.WriteLine(f2);
            Console.WriteLine(f3);

            var j = f2.SkipWhile(x => x["Date"].ToDateTime() < new DateTime(2009, 01, 01))
                .InnerJoin(f3, x => x["Date"].ToDateTime().Date, x => x["DATE"].ToDateTime().Date)
                .OrderBy(x => x["Date"]);

            Console.WriteLine(j);

            j.Drop("DATE", "OPEN", "HIGH", "LOW", "Open", "High", "Low", "Dividends", "Stock Splits");
            j.Rename("CLOSE", "VIX");
            j.Rename("Close", "SPX");
            j.Rename("Date", "Timestamp");
            
            Console.WriteLine("Volatility: " + f2["ReturnPct"].Volatility());

            //joined.PlotXY("Return", "ReturnPct");

            //joined.PlotHistogram("ReturnPct", -2, 2);

            j["SPX_RSI_14"] = j["SPX"].Rsi(14).Ema(14);

            j["VIX_MA30"] = j["VIX"].Sma(30);
            j["VIX_MA60"] = j["VIX"].Sma(60);
            j["VIX_MA120"] = j["VIX"].Sma(120);
            j["VIX_MA240"] = j["VIX"].Sma(240);
            j["VIX_MA1000"] = j["VIX"].Sma(1000);

            j["VIX_MA30_Diff"] = (j["VIX"] / j["VIX_MA30"]) - 1;
            j["VIX_MA60_Diff"] = (j["VIX"] / j["VIX_MA60"]) - 1;
            j["VIX_MA120_Diff"] = (j["VIX"] / j["VIX_MA120"]) - 1;
            j["VIX_MA240_Diff"] = (j["VIX"] / j["VIX_MA240"]) - 1;
            j["VIX_MA1000_Diff"] = (j["VIX"] / j["VIX_MA1000"]) - 1;

            j.PlotTimeSeries("VIX_MA30_Diff", "VIX_MA60_Diff", "VIX_MA120_Diff", "VIX_MA240_Diff");

            j["VIX_MA_ENS"] = j.EnsembleSignal(0, 
                "VIX_MA30_Diff",
                "VIX_MA60_Diff",
                "VIX_MA120_Diff",
                "VIX_MA240_Diff",
                "VIX_MA1000_Diff"
                );

            j["VIX_MA_ENS_MA"] = j["VIX_MA_ENS"].Sma(18);

            j["VIX_MA_ENS_MA"] = j["VIX_MA_ENS"].Window(100,0,window => 0);


            j.PlotTimeSeries("VIX_MA_ENS", "VIX_MA_ENS_MA");

            j["SPX_RSI_14_CrossUp"] = j["SPX_RSI_14"].Crossover(-0.05);

            Console.WriteLine(j["SPX_RSI_14_CrossUp"]);

            var previousRsi = 0d;
            j = j.Backtest("S1", "SPX", (x, engine) =>
            {
                if (x["VIX"] > 20)
                {
                    engine.SetPosition(0);
                    return;
                }

                if (x["VIX_MA_ENS_MA"] < 0.5)
                {
                    engine.SetPosition(1);
                    return;
                }

                if (x["SPX_RSI_14_CrossUp"] > 0)
                {
                    engine.SetPosition(1);
                    return;
                }

                if (x["SPX_RSI_14"] < -0.05)
                {
                    engine.SetPosition(0);
                    return;
                }

                if (x["SPX_RSI_14"] > 0.15)
                {
                    engine.SetPosition(0);
                    return;
                }

                previousRsi = x["SPX_RSI_14"];
            });

            j = j.Backtest("BH", "SPX", (x, engine) =>
            {
                engine.SetPosition(1);
            });

            j = j.Backtest("VIX1", "SPX", (x, engine) =>
            {
                if (x["VIX_MA_ENS_MA"] >= -0.5)
                {
                    engine.SetPosition(0);
                }
                else
                {
                    engine.SetPosition(1);
                }
            });

            j = j.Backtest("VIX2", "SPX", (x, engine) =>
            {
                if (x["VIX_MA_ENS_MA"] >= 0)
                {
                    engine.SetPosition(0);
                }
                else
                {
                    engine.SetPosition(1);
                }
            });

            j = j.Backtest("VIX3", "SPX", (x, engine) =>
            {
                if (x["VIX_MA_ENS_MA"] >= 0.5)
                {
                    engine.SetPosition(0);
                }
                else
                {
                    engine.SetPosition(1);
                }
            });

            Console.WriteLine(j);

            j.PlotTimeSeries("SPX_RSI_14");
            j.PlotTimeSeries("VIX", "VIX_MA30", "VIX_MA60", "VIX_MA120", "VIX_MA240");
            j.PlotTimeSeries(
                "S1_Equity", 
                "VIX1_Equity", 
                "VIX2_Equity", 
                "VIX3_Equity", 
                "BH_Equity"
                );

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }
}
