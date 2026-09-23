using System.Text.RegularExpressions;
using Domain.Enums.Common;
using JoomleagueImporter.Models;

namespace JoomleagueImporter.Import;

/// <summary>
/// Skill band inferred from a JoomLeague project name.
/// Division is the band (Liiga, Divari, PMT). Audience stays on <see cref="Category"/>.
/// </summary>
/// <param name="DivisionName">Shared division name, for example "Salibandy Liiga".</param>
/// <param name="Level">1 is the top band. Higher numbers are lower or side series.</param>
/// <param name="Category">Audience inferred from the project name.</param>
/// <param name="IsTournament">PMT, playoff and cup projects. Stored as seasons; the flag only picks a team's home division.</param>
internal readonly record struct ImportSeriesProfile(
    string DivisionName,
    int Level,
    TeamCategory Category,
    bool IsTournament);

/// <summary>
/// Maps a JoomLeague project name onto a division band instead of one division per sport.
/// </summary>
internal static partial class ImportSeriesResolver
{
    public static ImportSeriesProfile Resolve(string? projectName, string sportLabel)
    {
        string label = string.IsNullOrWhiteSpace(sportLabel) ? "Sarja" : sportLabel.Trim();
        (string band, int level, bool tournament) = Classify(projectName);
        return new ImportSeriesProfile(
            $"{label} {band}",
            level,
            TeamCategoryResolver.InferFromName(projectName),
            tournament);
    }

    /// <summary>
    /// Home division is the latest league project the team played in.
    /// Cup, PMT and playoff projects are used only when the team has no league project.
    /// </summary>
    public static ImportSeriesProfile HomeForTeam(FloorballImportSet set, int oldTeamId, string sportLabel)
    {
        List<ProjectImport> appearances = set.Projects
            .Where(project => project.Teams.Values.Any(team => team.Team.Id == oldTeamId))
            .ToList();
        if (appearances.Count == 0)
        {
            return new ImportSeriesProfile($"{sportLabel.Trim()} Sarja", 2, TeamCategory.Adult, false);
        }

        ProjectImport? league = appearances
            .Where(project => !Resolve(project.Project.Name, sportLabel).IsTournament)
            .OrderByDescending(project => project.Project.StartDate ?? DateTime.MinValue)
            .ThenByDescending(project => project.Project.Id)
            .FirstOrDefault();

        ProjectImport chosen = league ?? appearances
            .OrderByDescending(project => project.Project.StartDate ?? DateTime.MinValue)
            .ThenByDescending(project => project.Project.Id)
            .First();

        return Resolve(chosen.Project.Name, sportLabel);
    }

    private static (string Band, int Level, bool Tournament) Classify(string? name)
    {
        string normalized = name?.Trim().ToLowerInvariant() ?? string.Empty;
        if (PlayoffPattern().IsMatch(normalized))
            return ("Playoff", 3, true);
        if (PmtPattern().IsMatch(normalized))
            return ("PMT", 2, true);
        if (CupPattern().IsMatch(normalized))
            return ("Cup", 2, true);
        if (PlacementPattern().IsMatch(normalized))
            return ("Tasonmittaus", 4, false);
        if (VeteransPattern().IsMatch(normalized))
            return ("+40", 3, false);
        if (WinterPattern().IsMatch(normalized))
            return ("Talvisarja", 2, false);
        if (RautaliigaPattern().IsMatch(normalized))
            return ("Rautaliiga", 2, false);
        if (DivariPattern().IsMatch(normalized))
            return ("Divari", 2, false);
        if (LiigaPattern().IsMatch(normalized))
            return ("Liiga", 1, false);
        return ("Sarja", 2, false);
    }

    [GeneratedRegex(@"playoff|play-off|loppupe|pudotus", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex PlayoffPattern();

    [GeneratedRegex(@"\bpmt\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex PmtPattern();

    [GeneratedRegex(@"\bcup\b|turnaus", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex CupPattern();

    [GeneratedRegex(@"tasonmittaus", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex PlacementPattern();

    [GeneratedRegex(@"\+40|yli\s*40", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex VeteransPattern();

    [GeneratedRegex(@"talvisarja|talvijalka", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex WinterPattern();

    [GeneratedRegex(@"rautaliiga", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex RautaliigaPattern();

    [GeneratedRegex(@"\bdivari\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex DivariPattern();

    [GeneratedRegex(@"\bliiga\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex LiigaPattern();
}
