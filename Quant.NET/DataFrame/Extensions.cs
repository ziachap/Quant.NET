namespace Quant.NET.DataFrame
{
    public static class Extensions
    {
        public static Column ToColumn(this IEnumerable<double> values)
        {
            return new Column(values.ToArray());
        }

        public static Column ToColumn(this IEnumerable<int> values)
        {
            return new Column(values.Select(x => (double)x).ToArray());
        }

        public static Frame ToFrame(this IEnumerable<Row> rows)
        {
            return new Frame(rows.ToList(), rows.First().Select(y => y.Key).ToList());

            // TODO
            //return new Frame(rows.ToList(), rows.SelectMany(x => x.Select(y => y.Key)).Distinct().ToList());
        }
    }
}
