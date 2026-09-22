namespace Domain.Common;

/// <summary>
/// Calendar rules for the yearly player-licence cutoff in Europe/Helsinki.
/// February 29 is allowed as a configured day and falls back to 28 February in non-leap years.
/// </summary>
public static class PlayerLicenceCalendar
{
    public static TimeZoneInfo HelsinkiTimeZone { get; } = ResolveHelsinkiTimeZone();

    public static DateOnly TodayInHelsinki(DateTime utcNow)
    {
        DateTime utc = utcNow.Kind == DateTimeKind.Utc
            ? utcNow
            : DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(utc, HelsinkiTimeZone);
        return DateOnly.FromDateTime(local);
    }

    public static DateOnly ResolveCutoff(int year, int month, int day)
    {
        int actualDay = Math.Min(day, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, actualDay);
    }

    /// <summary>
    /// The most recently completed cutoff year as of <paramref name="today"/>:
    /// this calendar year when today is on or after the cutoff, otherwise last year.
    /// </summary>
    public static int CurrentCutoffYear(DateOnly today, int month, int day)
    {
        DateOnly cutoffThisYear = ResolveCutoff(today.Year, month, day);
        return today >= cutoffThisYear ? today.Year : today.Year - 1;
    }

    public static bool IsAutomaticResetDue(DateOnly today, int month, int day, int? lastResetYear)
    {
        if (!lastResetYear.HasValue)
        {
            return false;
        }

        DateOnly cutoffThisYear = ResolveCutoff(today.Year, month, day);
        if (today < cutoffThisYear)
        {
            return false;
        }

        return lastResetYear.Value != today.Year;
    }

    private static TimeZoneInfo ResolveHelsinkiTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Helsinki");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }
    }
}
