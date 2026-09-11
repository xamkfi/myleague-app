using System.Text.RegularExpressions;
using Domain.Enums.Common;

namespace JoomleagueImporter.Import;

/// <summary>
/// Infers <see cref="TeamCategory"/> from JoomLeague project / team names.
/// Uses Finnish and English keywords present in the historical MAHL dump
/// (e.g. "SALIBANDY PMT 2026 | NAISET", "… | MIEHET", junior markers).
/// Defaults to <see cref="TeamCategory.Adult"/> when nothing matches.
/// </summary>
internal static partial class TeamCategoryResolver
{
    // Women before Youth: explicit "naiset" wins over junior markers.
    private static readonly Regex WomenPattern = WomenRegex();
    private static readonly Regex YouthPattern = YouthRegex();

    public static TeamCategory InferFromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return TeamCategory.Adult;

        string normalized = name.Trim().ToLowerInvariant();

        if (WomenPattern.IsMatch(normalized))
            return TeamCategory.Women;

        if (YouthPattern.IsMatch(normalized))
            return TeamCategory.Youth;

        // Explicit men's markers (miehet/men) and everything else → Adult.
        // +40 / veterans have no dedicated category, so they stay Adult.
        return TeamCategory.Adult;
    }

    /// <summary>
    /// Prefer a non-Adult category when a team appears in multiple projects.
    /// Women and Youth both beat Adult; if both are present, Women wins.
    /// </summary>
    public static TeamCategory Prefer(TeamCategory current, TeamCategory candidate)
    {
        if (candidate == TeamCategory.Women)
            return TeamCategory.Women;
        if (candidate == TeamCategory.Youth && current != TeamCategory.Women)
            return TeamCategory.Youth;
        return current;
    }

    [GeneratedRegex(
        @"\b(naiset|naisten|nainen|ladies|women|woman)\b",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex WomenRegex();

    [GeneratedRegex(
        @"\b(youth|junior(?:it|s)?|nuoret|nuorten|pojat|poikien|tytöt|tytot|tyttöjen|tyttojen|u1[0-9]|u2[0-1]|c-?junior|b-?junior|a-?junior)\b",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex YouthRegex();
}
