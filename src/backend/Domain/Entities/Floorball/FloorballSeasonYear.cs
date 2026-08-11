namespace Domain.Entities.Floorball;

/// <summary>
/// Helpers for deriving and parsing floorball season-year labels (e.g. "2024-2025").
/// </summary>
public static class FloorballSeasonYear
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
            ? startYear.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{startYear}-{endYear}");
    }

    /// <summary>
    /// Parses a season-year label into start/end calendar years.
    /// </summary>
    public static bool TryParse(string? seasonYear, out int startYear, out int endYear)
    {
        startYear = 0;
        endYear = 0;

        if (string.IsNullOrWhiteSpace(seasonYear))
            return false;

        string trimmed = seasonYear.Trim();
        int dashIndex = trimmed.IndexOf('-', StringComparison.Ordinal);
        if (dashIndex > 0)
        {
            return int.TryParse(trimmed[..dashIndex], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out startYear)
                   && int.TryParse(trimmed[(dashIndex + 1)..], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out endYear);
        }

        if (!int.TryParse(trimmed, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out startYear))
            return false;

        endYear = startYear;
        return true;
    }
}
