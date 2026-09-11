using System.Globalization;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// Helpers for deriving and parsing football season-year labels (e.g. "2024-2025").
/// </summary>
public static class FootballSeasonYear
{
    public static string FromDates(DateTime startDate, DateTime endDate)
    {
        int startYear = startDate.Year;
        int endYear = endDate.Year;
        return startYear == endYear
            ? startYear.ToString(CultureInfo.InvariantCulture)
            : string.Create(CultureInfo.InvariantCulture, $"{startYear}-{endYear}");
    }

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
            return int.TryParse(trimmed[..dashIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out startYear)
                   && int.TryParse(trimmed[(dashIndex + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out endYear);
        }

        if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out startYear))
            return false;

        endYear = startYear;
        return true;
    }
}
