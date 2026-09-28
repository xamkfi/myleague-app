using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Domain.Services.Common;
using Domain.ValueObjects.Football;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Fills public standings with zero rows for teams enrolled in a started competition.
/// </summary>
internal static class FootballEnrolledStandings
{
    public static bool IsStarted(FootballCompetition competition)
    {
        if (competition is FootballTournament tournament)
        {
            return tournament.TournamentStatus is FootballTournamentStatus.GroupStage
                or FootballTournamentStatus.PlayoffStage
                or FootballTournamentStatus.Completed;
        }

        return competition.IsActive || competition.IsCompleted;
    }

    public static async Task<List<FootballTeamSeasonStatisticsDto>> WithEnrolledZerosAsync(
        Guid competitionId,
        IReadOnlyList<FootballTeamSeasonStatisticsDto> existing,
        IFootballCompetitionRepository competitions,
        IFootballTournamentRepository tournaments,
        IFootballTeamRepository teams,
        IClubRepository clubs,
        IFootballMatchRepository matches,
        CancellationToken cancellationToken)
    {
        FootballCompetition? competition = await competitions.GetByIdAsync(competitionId);
        List<FootballTeam> knownTeams = new();
        List<FootballTeamSeasonStatisticsDto> rows;
        if (competition is null || !IsStarted(competition))
        {
            rows = existing.ToList();
        }
        else
        {
            knownTeams = await LoadEnrolledTeamsAsync(competition, tournaments, teams, cancellationToken);
            rows = Merge(existing, knownTeams, competition.Id, competition.Name ?? string.Empty);
        }

        await ApplyMarksAsync(rows, knownTeams, teams, clubs, cancellationToken);
        IReadOnlyList<StandingSortCriterion> criteria = CriteriaFor(competition);
        IReadOnlyList<StandingMatchResult> played = await LoadResultsAsync(competition, criteria, matches);
        return StandingTableOrder.Sort(
            rows,
            criteria,
            row => new StandingSortSnapshot(
                row.TeamId,
                row.Points,
                row.GoalDifference,
                row.GoalsFor,
                row.GoalsAgainst,
                0,
                row.TeamName),
            played);
    }

    private static async Task<IReadOnlyList<StandingMatchResult>> LoadResultsAsync(
        FootballCompetition? competition,
        IReadOnlyList<StandingSortCriterion> criteria,
        IFootballMatchRepository matches)
    {
        if (competition is null || !StandingSortCriteria.UsesHeadToHead(criteria))
            return [];

        FootballStandingRules rules = competition.StandingRules;
        List<StandingMatchResult> results = new();
        foreach (FootballMatch match in await matches.GetByCompetitionIdAsync(competition.Id))
        {
            if (match.Status != FootballMatchStatus.Completed || match.PlayoffRound is not null)
                continue;
            if (match.HomeTeamId is not Guid homeId || match.AwayTeamId is not Guid awayId)
                continue;
            if (homeId == Guid.Empty || awayId == Guid.Empty)
                continue;

            int homePoints = match.HomeScore > match.AwayScore
                ? rules.WinPoints
                : match.HomeScore == match.AwayScore ? rules.DrawPoints : rules.LossPoints;
            int awayPoints = match.AwayScore > match.HomeScore
                ? rules.WinPoints
                : match.HomeScore == match.AwayScore ? rules.DrawPoints : rules.LossPoints;
            results.Add(new StandingMatchResult(homeId, awayId, match.HomeScore, match.AwayScore, homePoints, awayPoints));
        }

        return results;
    }

    private static IReadOnlyList<StandingSortCriterion> CriteriaFor(FootballCompetition? competition)
    {
        if (competition is FootballSeason season && season.RankingCriteria.Count > 0)
            return season.RankingCriteria;

        return StandingSortCriteria.LegacyWithoutGoalsAgainst;
    }

    private static async Task<List<FootballTeam>> LoadEnrolledTeamsAsync(
        FootballCompetition competition,
        IFootballTournamentRepository tournaments,
        IFootballTeamRepository teams,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, FootballTeam> enrolled = new();
        foreach (FootballTeam team in competition.Teams)
            enrolled[team.Id] = team;

        if (competition is not FootballTournament)
            return enrolled.Values.ToList();

        FootballTournament? withGroups = await tournaments.GetByIdWithGroupsAsNoTrackingAsync(competition.Id, cancellationToken);
        if (withGroups is null)
            return enrolled.Values.ToList();

        foreach (FootballTournamentGroup group in withGroups.Groups)
        {
            foreach (FootballTournamentGroupTeam membership in group.Teams)
            {
                if (membership.Team is not null)
                {
                    enrolled[membership.Team.Id] = membership.Team;
                    continue;
                }

                if (enrolled.ContainsKey(membership.TeamId))
                    continue;

                FootballTeam? loaded = await teams.GetByIdAsync(membership.TeamId);
                if (loaded is not null)
                    enrolled[loaded.Id] = loaded;
            }
        }

        return enrolled.Values.ToList();
    }

    private static List<FootballTeamSeasonStatisticsDto> Merge(
        IReadOnlyList<FootballTeamSeasonStatisticsDto> existing,
        IReadOnlyList<FootballTeam> enrolled,
        Guid competitionId,
        string seasonName)
    {
        HashSet<Guid> present = existing.Select(row => row.TeamId).ToHashSet();
        List<FootballTeamSeasonStatisticsDto> merged = existing.ToList();
        foreach (FootballTeam team in enrolled.Where(team => present.Add(team.Id)))
        {
            merged.Add(new FootballTeamSeasonStatisticsDto
            {
                TeamId = team.Id,
                CompetitionId = competitionId,
                TeamName = team.Name ?? string.Empty,
                TeamLogo = team.LogoUrl,
                SeasonName = seasonName
            });
        }

        return merged;
    }

    private static async Task ApplyMarksAsync(
        List<FootballTeamSeasonStatisticsDto> rows,
        IReadOnlyList<FootballTeam> knownTeams,
        IFootballTeamRepository teams,
        IClubRepository clubs,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return;

        Dictionary<Guid, FootballTeam> byId = new();
        foreach (FootballTeam team in knownTeams)
            byId[team.Id] = team;

        foreach (Guid teamId in rows.Select(row => row.TeamId).Distinct())
        {
            if (byId.ContainsKey(teamId))
                continue;

            FootballTeam? loaded = await teams.GetByIdAsync(teamId);
            if (loaded is not null)
                byId[loaded.Id] = loaded;
        }

        List<Guid> clubIds = byId.Values.Select(team => team.ClubId).Distinct().ToList();
        Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
            ? new Dictionary<Guid, Club>()
            : await clubs.GetByIdsAsync(clubIds, cancellationToken);

        foreach (FootballTeamSeasonStatisticsDto row in rows)
        {
            if (!byId.TryGetValue(row.TeamId, out FootballTeam? team))
                continue;

            clubLookup.TryGetValue(team.ClubId, out Club? club);
            row.TeamLogo = PublicLogoUrl.OmitPlaceholder(team.GetEffectiveLogoUrl(club?.LogoUrl));
            row.TeamShortName = team.ShortName;
        }
    }
}
