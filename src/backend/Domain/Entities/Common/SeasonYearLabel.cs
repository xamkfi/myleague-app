using System.Globalization;

namespace Domain.Entities.Common;

/// <summary>
/// Helpers for deriving season-year labels (e.g. "2024" or "2024-2025") from dates.
/// </summary>
public static class SeasonYearLabel
{
    /// <summary>
    /// Builds a season-year label from start/end dates.
    /// Same calendar year → "2024"; spanning years → "2024-2025".
    /// </summary>
    public static string FromDates(DateTime startDate, DateTime endDate)
    {
        int startYear = startDate.Year;
        int endYear = endDate.Year;
        return startYear == endYear
            ? startYear.ToString(CultureInfo.InvariantCulture)
            : string.Create(CultureInfo.InvariantCulture, $"{startYear}-{endYear}");
    }
}
