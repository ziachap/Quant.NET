using Quant.NET.DataFrame;

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

            Console.WriteLine(f2);
            Console.WriteLine(f3);

            var joined = f2.InnerJoin(f3, x => x["Date"].ToDateTime().Date, x => x["DATE"].ToDateTime().Date)
                .OrderBy(x => x["Date"]);

            joined.Drop("DATE", "OPEN", "HIGH", "LOW", "Open", "High", "Low", "Dividends", "Stock Splits");
            joined.Rename("CLOSE", "VIX");
            joined.Rename("Close", "SPX");
            
            Console.WriteLine(joined);

            var subset = joined["Date", "SPX"];

            Console.WriteLine(subset);

            Console.WriteLine("Volatility: " + f2["ReturnPct"].Volatility());

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }
}
