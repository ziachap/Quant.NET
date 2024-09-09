namespace Quant.NET;

public static class DateTimeExtensions
{
    /// <summary>
    /// Assumes DateTime is UTC.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>UNIX timestamp in seconds</returns>
    public static double DateTimeToUnixTimeStampUTC(this DateTime dateTime)
    {
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var unixTimestamp = (dateTime - epoch).TotalSeconds;
        return unixTimestamp;
    }

    /// <param name="dateTime"></param>
    /// <returns>UNIX timestamp in seconds</returns>
    public static double DateTimeToUnixTimeStamp(this DateTime dateTime)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        var unixTimestamp = (dateTime - epoch).TotalSeconds;
        return unixTimestamp;
    }

    /// <summary>
    /// Assumes UNIX timestamp is UTC.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime UnixTimeStampToDateTimeUTC(this double unixTimeStampSeconds)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStampSeconds);
        return dateTime;
    }

    public static DateTime UnixTimeStampToDateTime(this double unixTimeStampSeconds)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStampSeconds).ToLocalTime();
        return dateTime;
    }

    public static DateTime UnixTimeStampToDateTime(this long unixTimeStampNanoseconds)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddMicroseconds(unixTimeStampNanoseconds / 1000d).ToLocalTime();
        return dateTime;
    }

    /// <summary>
    /// Assumes UNIX timestamp is UTC.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime UnixTimeStampToDateTimeUTC(this long unixTimeStampNanoseconds)
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddMicroseconds(unixTimeStampNanoseconds / 1000d);
        return dateTime;
    }
}