namespace BuildingBlocks.Core.Utilities;

/// <summary>
/// Provides helper methods for working with date ranges.
/// </summary>
public static class DateTimeUtilities
{
    /// <summary>
    /// Returns each calendar day in the inclusive range between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    public static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
    {
        for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
            yield return day;
    }
}
