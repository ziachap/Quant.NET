# Quant.NET
.NET tools for quantitative research

Quant.NET provides tools to assist in quantitative research/trading in C#. Features include:
* Dataframes
* Indicators & statistical calculations
* Backtesting
* Metrics

Coming soon:
* SIMD support for vector based arithmetic

## Dataframes

You can create dataframes manually:
```
var f = new Frame();
```
Frames store two dimensional non-nullable numeric data. They have rows and named columns. As rows and columns are manipulated, the Frame will update its schema and maintain consistency throughout the structure.

Adding new columns:
```
f["Apple"] = new Column([1, 2, 3]);
f["Banana"] = new Column([2.5, 4.6, 2.3]);
```
Adding new rows:
```
var row = f.CreateRow();

row["Banana"] = 2;

Console.WriteLine(f);
```

```
Output:
Apple   Banana
1.000   2.500
2.000   4.600
3.000   2.300
0.000   2.000
4 Rows
```

You can also perform arithmetic on columns:
```
f["Multiplied"] = f["Apple"] * f["Banana"]
```

### Loading data from a CSV

Typically you will want to load data from a data srouce. Currently only loading from CSV is supported:
```
var f = Frame.LoadCsv("C:\\data_folder\\data.csv", dateTimeFormat: "yyyy-MM-dd");
```
* CSV files should have column headers in the first row.
* Frame will parse the 1st row of data (2nd row in the file) and make a note of columns that contains numeric or DateTime data. It will only attempt to read data from these columns, other columns will be ignored as they are not supported.
* Datetime data will parsed according to the provided DateTime format.
* Cells which contain unparseable data (e.g. nulls) will output 0.

## Enumerable support
Frame, Row and Column implement IEnumerable:
* Frame will iterate through rows.
* Row will iterate through key value pairs of columns and their values
* Column will iterate through the column's values

Using LINQ extension methods on column data:
```
var sum = f["Banana"].Sum();
var average = f["Banana"].Average();
```

There are many other useful functions within the library, so I recommend you download it and try it for yourself!

## Backtesting
You can perform simple backtests on your data very quickly with a few lines of code:
```
f = f.Backtest("Strategy_Name", "Price_Column", (row, engine) =>
{
    if (row["MyParameter"] > 5)
    {
        engine.SetPosition(1);
    }
    else
    {
        engine.SetPosition(0);
    }
});
```
The backtesting engine will use the price of the column you specify ("Price_Column") to calcualte the equity throughout the strategy. 
* `row` provides access to the row of data for the current slice in time.
* `engine` provides functions to set your position and place trades.

The backtesting engine will add new columns to the resulting dataframe reprenting key metrics throughout the strategy (e.g. Equity). You can use this to analyse your equity curve.


