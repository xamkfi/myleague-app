using System.Text.RegularExpressions;

namespace JoomleagueImporter.Import;

/// <summary>
/// Parses JoomLeague event clock strings shared by all sport importers.
/// </summary>
internal static class ImportTimeParser
{
    private static readonly Regex TimePattern = new(@"^(\d{1,3})\s*[:.,]\s*(\d{1,2})$", RegexOptions.Compiled);
    private static readonly Regex MinutesOnlyPattern = new(@"^(\d{1,3})$", RegexOptions.Compiled);

    /// <summary>
    /// Parses JoomLeague event times like "13:52", "15.08" or "27" (minutes) into seconds on
    /// the continuous match clock. Returns null when the value is empty or unparseable.
    /// </summary>
    public static int? ParseEventTime(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        string value = raw.Trim();

        Match m = TimePattern.Match(value);
        if (m.Success)
        {
            int minutes = int.Parse(m.Groups[1].Value);
            int seconds = int.Parse(m.Groups[2].Value);
            if (seconds > 59)
                seconds = 59;
            return minutes * 60 + seconds;
        }

        m = MinutesOnlyPattern.Match(value);
        if (m.Success)
            return int.Parse(m.Groups[1].Value) * 60;

        return null;
    }
}
