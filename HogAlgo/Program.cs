// See https://aka.ms/new-console-template for more information


using Quant.NET.DataFrame;

Console.WriteLine("Hello, World!");


var f2 = Frame.LoadCsv("C:\\data\\csv\\spx\\SPX.csv");
var f3 = Frame.LoadCsv("C:\\data\\csv\\vix\\VIX_History.csv", "MM/dd/yyyy");