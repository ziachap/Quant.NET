using System;
using Quant.NET.DataFrame;
using ScottPlot.WinForms;
using ScottPlot;
using ScottPlot.Statistics;
using System.Data.Common;

namespace Quant.NET.Plots
{
    public static class PlotHelpers
    {
        private static bool IsPolyglotNotebook => AppDomain.CurrentDomain.GetAssemblies()
            .Any(assembly => assembly.FullName.Contains("Microsoft.DotNet.Interactive") ||
                             assembly.FullName.Contains("Polyglot.Notebooks"));

        public static Plot PlotTimeSeries(this Frame f, params string[] columns)
        {
            var plot = new Plot();

            foreach (var column in columns)
            {
                if (f.ContainsColumn("Timestamp"))
                {
                    var dataX = f.Select(x => x["Timestamp"].ToDateTime()).ToArray();
                    var dataY = f.Select(x => x[column]).ToArray();
                    var series = plot.Add.SignalXY(dataX, dataY);
                    series.LegendText = column;
                }
                else
                {
                    var data = f.Select(x => x[column]).ToArray();
                    var series = plot.Add.Signal(data);
                    series.LegendText = column;
                }
            }

            if (f.ContainsColumn("Timestamp"))
            {
                plot.Axes.DateTimeTicksBottom();
            }

            plot.ShowLegend();

            if (!IsPolyglotNotebook)
                Task.Run(() => FormsPlotViewer.Launch(plot, "Plot", 1200, 600));

            return plot;
        }

        public static Plot PlotXY(this Frame f, string xColumn, string yColumn)
        {
            var plot = new Plot();

            var ordered = f.OrderBy(x => x[xColumn]).ToArray();

            var dataX = ordered.Select(x => x[xColumn]).ToArray();
            var dataY = ordered.Select(x => x[yColumn]).ToArray();

            var series = plot.Add.Scatter(dataX, dataY);
            series.LineWidth = 0;
            series.MarkerShape = MarkerShape.Cross;

            series.LegendText = $"X: {xColumn}, Y: {yColumn}";

            LinearRegression reg = new(dataX, dataY);
            Coordinates pt1 = new(dataX.First(), reg.GetValue(dataX.First()));
            Coordinates pt2 = new(dataX.Last(), reg.GetValue(dataX.Last()));
            var line = plot.Add.Line(pt1, pt2);
            line.MarkerSize = 0;
            line.LineWidth = 2;
            line.LinePattern = LinePattern.Dashed;

            //plot.Axes.DateTimeTicksBottom();
            plot.ShowLegend();

            if (!IsPolyglotNotebook)
                Task.Run(() => FormsPlotViewer.Launch(plot, "XY Plot", 1200, 600));

            return plot;
        }

        public static Plot PlotHistogram(this Frame f, string column, double min, double max, int bins = 100)
        {
            var plot = new Plot();

            var data = f.Select(x => x[column]).ToArray();
            
            var hist = new Histogram(min, max, bins);
            hist.AddRange(data);

            var series = plot.Add.Lollipop(hist.Counts, hist.Bins);

            series.LegendText = column;

            //plot.Axes.DateTimeTicksBottom();
            plot.ShowLegend();

            if (!IsPolyglotNotebook)
                Task.Run(() => FormsPlotViewer.Launch(plot, "XY Plot", 1200, 600));

            return plot;
        }
    }
}
