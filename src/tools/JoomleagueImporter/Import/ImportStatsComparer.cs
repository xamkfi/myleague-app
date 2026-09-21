using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Hockey.Statistics.DTOs;

namespace JoomleagueImporter.Import;

public sealed class ImportCompareReport
{
    public DateTime ComparedAtUtc { get; set; } = DateTime.UtcNow;
    public int SeasonsCompared { get; set; }
    public int Differences { get; set; }
    public List<int> ImportFailedMatchIds { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}

public static class ImportStatsComparer
{
    public static async Task<ImportCompareReport> CompareFloorballAsync(
        ImportExpectedReport expected,
        FloorballApiClient api)
    {
        ImportCompareReport report = NewReport(expected);
        foreach (ExpectedSeasonStats season in expected.Seasons.Where(s => s.NewSeasonId.HasValue))
        {
            report.SeasonsCompared++;
            Guid seasonId = season.NewSeasonId!.Value;
            List<FloorballTeamSeasonStatisticsDto> standings = await api.GetStandingsAsync(seasonId);
            CompareTeams(report, season, standings.Select(ToRow).ToList(), tiesFromDraws: false);

            foreach (ExpectedTeamStats team in season.Teams.Where(t => t.NewTeamId.HasValue))
            {
                List<FloorballPlayerSeasonStatisticsDto> players =
                    await api.GetTeamPlayerStatisticsAsync(seasonId, team.NewTeamId!.Value);
                ComparePlayers(
                    report,
                    season,
                    team,
                    players.Select(p => new ActualPlayerRow(p.PlayerId, p.Goals, p.Assists, p.PenaltyMinutes, 0, 0)));
            }
        }

        return Finish(report);
    }

    public static async Task<ImportCompareReport> CompareFootballAsync(
        ImportExpectedReport expected,
        FootballApiClient api)
    {
        ImportCompareReport report = NewReport(expected);
        foreach (ExpectedSeasonStats season in expected.Seasons.Where(s => s.NewSeasonId.HasValue))
        {
            report.SeasonsCompared++;
            Guid seasonId = season.NewSeasonId!.Value;
            List<FootballTeamSeasonStatisticsDto> standings = await api.GetStandingsAsync(seasonId);
            CompareTeams(
                report,
                season,
                standings.Select(s => new ActualTeamRow(
                    s.TeamId, s.GamesPlayed, s.Wins, s.Draws, s.Losses, s.Points, s.GoalsFor, s.GoalsAgainst)).ToList(),
                tiesFromDraws: true);

            foreach (ExpectedTeamStats team in season.Teams.Where(t => t.NewTeamId.HasValue))
            {
                List<FootballPlayerSeasonStatisticsDto> players =
                    await api.GetTeamPlayerStatisticsAsync(seasonId, team.NewTeamId!.Value);
                ComparePlayers(
                    report,
                    season,
                    team,
                    players.Select(p => new ActualPlayerRow(p.PlayerId, p.Goals, p.Assists, 0, p.YellowCards, p.RedCards)));
            }
        }

        return Finish(report);
    }

    public static async Task<ImportCompareReport> CompareHockeyAsync(
        ImportExpectedReport expected,
        HockeyApiClient api)
    {
        ImportCompareReport report = NewReport(expected);
        foreach (ExpectedSeasonStats season in expected.Seasons.Where(s => s.NewSeasonId.HasValue))
        {
            report.SeasonsCompared++;
            Guid seasonId = season.NewSeasonId!.Value;
            List<HockeyTeamCompetitionStatisticsDto> standings = await api.GetStandingsAsync(seasonId);
            CompareTeams(report, season, standings.Select(ToRow).ToList(), tiesFromDraws: false);

            List<HockeyPlayerCompetitionStatisticsDto> players = await api.GetPlayerStatisticsAsync(seasonId);
            foreach (ExpectedTeamStats team in season.Teams.Where(t => t.NewTeamId.HasValue))
            {
                IEnumerable<ActualPlayerRow> teamPlayers = players
                    .Where(p => p.TeamId == team.NewTeamId)
                    .Select(p => new ActualPlayerRow(p.PlayerId, p.Goals, p.Assists, p.PenaltyMinutes, 0, 0));
                ComparePlayers(report, season, team, teamPlayers);
            }
        }

        return Finish(report);
    }

    private static ImportCompareReport NewReport(ImportExpectedReport expected) =>
        new() { ImportFailedMatchIds = [.. expected.ImportFailedMatchIds] };

    private static ImportCompareReport Finish(ImportCompareReport report)
    {
        if (report.Differences == 0)
            report.Notes.Add("No standing or scoring differences for mapped seasons.");
        return report;
    }

    private static ActualTeamRow ToRow(FloorballTeamSeasonStatisticsDto row) =>
        new(row.TeamId, row.GamesPlayed, row.Wins, row.Ties, row.Losses, row.Points, row.GoalsFor, row.GoalsAgainst);

    private static ActualTeamRow ToRow(HockeyTeamCompetitionStatisticsDto row) =>
        new(row.TeamId, row.GamesPlayed, row.Wins, row.Ties, row.Losses, row.Points, row.GoalsFor, row.GoalsAgainst);

    private static void CompareTeams(
        ImportCompareReport report,
        ExpectedSeasonStats season,
        List<ActualTeamRow> actual,
        bool tiesFromDraws)
    {
        _ = tiesFromDraws;
        Dictionary<Guid, ActualTeamRow> byId = actual
            .GroupBy(r => r.TeamId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (ExpectedTeamStats team in season.Teams.Where(t => t.NewTeamId.HasValue))
        {
            if (!byId.TryGetValue(team.NewTeamId!.Value, out ActualTeamRow? row))
            {
                report.Differences++;
                report.Notes.Add($"{season.Name}: team '{team.Name}' missing from API standings.");
                continue;
            }

            AddIfDifferent(report, season.Name, team.Name, "GP", team.GamesPlayed, row.GamesPlayed);
            AddIfDifferent(report, season.Name, team.Name, "W", team.Wins, row.Wins);
            AddIfDifferent(report, season.Name, team.Name, "T", team.Ties, row.Ties);
            AddIfDifferent(report, season.Name, team.Name, "L", team.Losses, row.Losses);
            AddIfDifferent(report, season.Name, team.Name, "Pts", team.Points, row.Points);
            AddIfDifferent(report, season.Name, team.Name, "GF", team.GoalsFor, row.GoalsFor);
            AddIfDifferent(report, season.Name, team.Name, "GA", team.GoalsAgainst, row.GoalsAgainst);
        }
    }

    private static void ComparePlayers(
        ImportCompareReport report,
        ExpectedSeasonStats season,
        ExpectedTeamStats team,
        IEnumerable<ActualPlayerRow> actual)
    {
        Dictionary<Guid, ActualPlayerRow> byId = actual
            .Where(p => p.PlayerId != Guid.Empty)
            .GroupBy(p => p.PlayerId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (ExpectedPlayerStats player in season.Players
            .Where(p => p.OldTeamId == team.OldTeamId && p.NewPlayerId.HasValue && p.Goals + p.Assists > 0))
        {
            if (!byId.TryGetValue(player.NewPlayerId!.Value, out ActualPlayerRow? row))
            {
                report.Differences++;
                report.Notes.Add($"{season.Name}/{team.Name}: scorer '{player.Name}' missing from API stats.");
                continue;
            }

            AddIfDifferent(report, season.Name, player.Name, "G", player.Goals, row.Goals);
            AddIfDifferent(report, season.Name, player.Name, "A", player.Assists, row.Assists);
        }
    }

    private static void AddIfDifferent(
        ImportCompareReport report,
        string season,
        string subject,
        string field,
        int expected,
        int actual)
    {
        if (expected == actual)
            return;
        report.Differences++;
        report.Notes.Add($"{season}/{subject}: {field} expected {expected}, API {actual}.");
    }

    private sealed record ActualTeamRow(
        Guid TeamId,
        int GamesPlayed,
        int Wins,
        int Ties,
        int Losses,
        int Points,
        int GoalsFor,
        int GoalsAgainst);

    private sealed record ActualPlayerRow(
        Guid PlayerId,
        int Goals,
        int Assists,
        int PenaltyMinutes,
        int YellowCards,
        int RedCards);
}
