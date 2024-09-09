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

            f2["Return"] = f2["Close"].Diff();
            f2["ReturnPct"] = f2["Close"].DiffPct();

            Console.WriteLine(f2);

            Console.WriteLine("Volatility: " + f2["ReturnPct"].Volatility());

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }
    }
}
