using Application.Features.Hockey.Statistics.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;
using Domain.Services.Common;
using Domain.Services.Hockey;
using Domain.ValueObjects.Hockey.Rules;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Shared helpers for hockey statistics handlers.
/// </summary>
internal static class HockeyStatisticsHandlerSupport
{
    public static async Task AttachTeamPlayersAsync(
        HockeyMatch match,
        IHockeyTeamRepository teamRepository)
    {
        Dictionary<Guid, HockeyTeam> teamCache = new();

        foreach (HockeyMatchTeam matchTeam in match.MatchTeams)
        {
            if (matchTeam.PlayerSelection is null)
                continue;

            if (!teamCache.TryGetValue(matchTeam.TeamId, out HockeyTeam? team))
            {
                team = await teamRepository.GetByIdAsync(matchTeam.TeamId);
                if (team is null)
                    continue;
                teamCache[matchTeam.TeamId] = team;
            }

            Dictionary<Guid, HockeyTeamPlayer> rosterById = team.Roster.ToDictionary(r => r.Id);
            foreach (HockeyMatchActivePlayer active in matchTeam.PlayerSelection.ActivePlayers)
            {
                if (rosterById.TryGetValue(active.TeamPlayerId, out HockeyTeamPlayer? teamPlayer))
                    active.AttachTeamPlayer(teamPlayer);
            }
        }
    }

    public static bool MatchesScope(
        HockeyMatch match,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId) =>
        scope switch
        {
            HockeyStatisticsScope.Competition => true,
            HockeyStatisticsScope.Division => match.CompetitionDivisionId == competitionDivisionId,
            HockeyStatisticsScope.TournamentGroup => match.TournamentGroupId == tournamentGroupId,
            HockeyStatisticsScope.PlayoffSeries => match.PlayoffSeriesId == playoffSeriesId,
            _ => false
        };

    public static void ValidateScopeIds(
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId,
        Guid? tournamentGroupId,
        Guid? playoffSeriesId)
    {
        switch (scope)
        {
            case HockeyStatisticsScope.Competition:
                if (competitionDivisionId is not null || tournamentGroupId is not null || playoffSeriesId is not null)
                    throw new InvalidOperationException("Competition scope cannot reference division, group or playoff series.");
                break;
            case HockeyStatisticsScope.Division:
                if (competitionDivisionId is null)
                    throw new InvalidOperationException("Division scope requires a competition division id.");
                if (tournamentGroupId is not null || playoffSeriesId is not null)
                    throw new InvalidOperationException("Division scope cannot reference tournament group or playoff series.");
                break;
            case HockeyStatisticsScope.TournamentGroup:
                if (tournamentGroupId is null)
                    throw new InvalidOperationException("Tournament group scope requires a tournament group id.");
                if (competitionDivisionId is not null || playoffSeriesId is not null)
                    throw new InvalidOperationException("Tournament group scope cannot reference division or playoff series.");
                break;
            case HockeyStatisticsScope.PlayoffSeries:
                if (playoffSeriesId is null)
                    throw new InvalidOperationException("Playoff series scope requires a playoff series id.");
                if (competitionDivisionId is not null || tournamentGroupId is not null)
                    throw new InvalidOperationException("Playoff series scope cannot reference division or tournament group.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unknown statistics scope.");
        }
    }

    public static void AssignStandingRanks(
        IList<Domain.Entities.Hockey.Statistics.HockeyTeamCompetitionStatistics> teams,
        IReadOnlyList<HockeyTieBreakerRule> tieBreakers)
    {
        List<Domain.Entities.Hockey.Statistics.HockeyTeamCompetitionStatistics> ordered = HockeyStandingTableOrder.Sort(
            teams,
            tieBreakers,
            team => new HockeyStandingSortSnapshot(
                team.Points,
                team.RegulationWins,
                team.Wins,
                team.GoalDifference,
                team.GoalsFor,
                team.GoalsAgainst,
                team.PenaltyMinutes,
                team.Team?.Name ?? string.Empty));

        for (int i = 0; i < ordered.Count; i++)
            ordered[i].SetStandingRank(i + 1);
    }

    public static bool IsStarted(HockeyCompetition competition) =>
        competition.IsActive || competition.IsCompleted;

    public static List<Guid> ActiveCompetitionTeamIds(HockeyCompetition competition) =>
        competition.Teams
            .Where(team => team.IsActive)
            .Select(team => team.TeamId)
            .Distinct()
            .ToList();

    public static List<Guid> ActiveGroupTeamIds(HockeyTournament tournament, Guid groupId)
    {
        HockeyTournamentGroup? group = tournament.Groups.FirstOrDefault(item => item.Id == groupId);
        if (group is null)
            return [];

        Dictionary<Guid, HockeyCompetitionTeam> members = tournament.Teams.ToDictionary(team => team.Id);
        return group.Teams
            .Where(membership => membership.IsActive && members.ContainsKey(membership.CompetitionTeamId))
            .Select(membership => members[membership.CompetitionTeamId])
            .Where(member => member.IsActive)
            .Select(member => member.TeamId)
            .Distinct()
            .ToList();
    }

    public static async Task<List<HockeyTeamCompetitionStatisticsDto>> WithEnrolledZerosAsync(
        HockeyCompetition? competition,
        IReadOnlyList<HockeyTeamCompetitionStatisticsDto> existing,
        IReadOnlyList<Guid> enrolledTeamIds,
        IHockeyTeamRepository teams,
        IHockeyMatchRepository matches,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? tournamentGroupId,
        CancellationToken cancellationToken)
    {
        if (competition is null || !IsStarted(competition))
            return existing.ToList();

        HashSet<Guid> nameIds = existing.Select(row => row.TeamId).ToHashSet();
        foreach (Guid teamId in enrolledTeamIds)
            nameIds.Add(teamId);

        IReadOnlyDictionary<Guid, string> names = nameIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await teams.GetNamesByIdsAsync(nameIds.ToList(), cancellationToken);

        if (competition is HockeySeason season)
        {
            IReadOnlyList<StandingSortCriterion> criteria = season.RankingCriteria.Count > 0
                ? season.RankingCriteria
                : StandingSortCriteria.Default;
            IReadOnlyList<StandingMatchResult> played = await LoadResultsAsync(
                competitionId,
                criteria,
                season.GetEffectiveRules().StandingRules,
                matches);
            return SortSeason(existing, enrolledTeamIds, names, competitionId, scope, tournamentGroupId, criteria, played);
        }

        return MergeZeros(
            existing,
            enrolledTeamIds,
            names,
            competitionId,
            scope,
            tournamentGroupId,
            competition.GetEffectiveRules().StandingRules.TieBreakers);
    }

    private static async Task<IReadOnlyList<StandingMatchResult>> LoadResultsAsync(
        Guid competitionId,
        IReadOnlyList<StandingSortCriterion> criteria,
        HockeyStandingRules rules,
        IHockeyMatchRepository matches)
    {
        if (!StandingSortCriteria.UsesHeadToHead(criteria))
            return [];

        List<StandingMatchResult> results = new();
        IEnumerable<HockeyMatch> validMatches = (await matches.GetByCompetitionIdAsync(competitionId))
            .Where(match => match.PlayoffSeriesId is null
                && match.CountsTowardStandings
                && match.Status is HockeyMatchStatus.Finished or HockeyMatchStatus.Forfeit
                && match.HomeTeamId is Guid homeId && homeId != Guid.Empty
                && match.AwayTeamId is Guid awayId && awayId != Guid.Empty);
        foreach (HockeyMatch match in validMatches)
        {
            Guid homeId = match.HomeTeamId!.Value;
            Guid awayId = match.AwayTeamId!.Value;
            (int homePoints, int awayPoints) = PointsFor(match, rules);
            results.Add(new StandingMatchResult(homeId, awayId, match.HomeScore, match.AwayScore, homePoints, awayPoints));
        }

        return results;
    }

    private static (int HomePoints, int AwayPoints) PointsFor(HockeyMatch match, HockeyStandingRules rules)
    {
        return match.ResultType switch
        {
            HockeyMatchResultType.HomeWin or HockeyMatchResultType.ForfeitHomeWin => (rules.RegulationWinPoints, 0),
            HockeyMatchResultType.AwayWin or HockeyMatchResultType.ForfeitAwayWin => (0, rules.RegulationWinPoints),
            HockeyMatchResultType.Draw => (rules.TiePoints, rules.TiePoints),
            HockeyMatchResultType.OvertimeHomeWin => (rules.OvertimeWinPoints, rules.OvertimeLossPoints),
            HockeyMatchResultType.OvertimeAwayWin => (rules.OvertimeLossPoints, rules.OvertimeWinPoints),
            HockeyMatchResultType.ShootoutHomeWin => (rules.ShootoutWinPoints, rules.ShootoutLossPoints),
            HockeyMatchResultType.ShootoutAwayWin => (rules.ShootoutLossPoints, rules.ShootoutWinPoints),
            _ when match.HomeScore > match.AwayScore => (rules.RegulationWinPoints, 0),
            _ when match.AwayScore > match.HomeScore => (0, rules.RegulationWinPoints),
            _ => (rules.TiePoints, rules.TiePoints)
        };
    }

    private static List<HockeyTeamCompetitionStatisticsDto> SortSeason(
        IReadOnlyList<HockeyTeamCompetitionStatisticsDto> existing,
        IReadOnlyList<Guid> enrolledTeamIds,
        IReadOnlyDictionary<Guid, string> names,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? tournamentGroupId,
        IReadOnlyList<StandingSortCriterion> criteria,
        IReadOnlyList<StandingMatchResult> matches)
    {
        List<HockeyTeamCompetitionStatisticsDto> merged = MergeRows(existing, enrolledTeamIds, names, competitionId, scope, tournamentGroupId);
        List<HockeyTeamCompetitionStatisticsDto> ordered = StandingTableOrder.Sort(
            merged,
            criteria,
            row => new StandingSortSnapshot(
                row.TeamId,
                row.Points,
                row.GoalDifference,
                row.GoalsFor,
                row.GoalsAgainst,
                row.PenaltyMinutes,
                row.TeamName),
            matches);

        for (int index = 0; index < ordered.Count; index++)
            ordered[index].StandingRank = index + 1;

        return ordered;
    }

    private static List<HockeyTeamCompetitionStatisticsDto> MergeZeros(
        IReadOnlyList<HockeyTeamCompetitionStatisticsDto> existing,
        IReadOnlyList<Guid> enrolledTeamIds,
        IReadOnlyDictionary<Guid, string> names,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? tournamentGroupId,
        IReadOnlyList<HockeyTieBreakerRule> tieBreakers)
    {
        List<HockeyTeamCompetitionStatisticsDto> merged = MergeRows(
            existing,
            enrolledTeamIds,
            names,
            competitionId,
            scope,
            tournamentGroupId);

        List<HockeyTeamCompetitionStatisticsDto> ordered = HockeyStandingTableOrder.Sort(
            merged,
            tieBreakers,
            row => new HockeyStandingSortSnapshot(
                row.Points,
                row.RegulationWins,
                row.Wins,
                row.GoalDifference,
                row.GoalsFor,
                row.GoalsAgainst,
                row.PenaltyMinutes,
                row.TeamName));

        for (int index = 0; index < ordered.Count; index++)
            ordered[index].StandingRank = index + 1;

        return ordered;
    }

    private static List<HockeyTeamCompetitionStatisticsDto> MergeRows(
        IReadOnlyList<HockeyTeamCompetitionStatisticsDto> existing,
        IReadOnlyList<Guid> enrolledTeamIds,
        IReadOnlyDictionary<Guid, string> names,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? tournamentGroupId)
    {
        HashSet<Guid> present = existing.Select(row => row.TeamId).ToHashSet();
        List<HockeyTeamCompetitionStatisticsDto> merged = existing.ToList();
        foreach (HockeyTeamCompetitionStatisticsDto row in merged.Where(row => names.ContainsKey(row.TeamId)))
            row.TeamName = names[row.TeamId];

        foreach (Guid teamId in enrolledTeamIds.Where(teamId => present.Add(teamId)))
        {
            merged.Add(new HockeyTeamCompetitionStatisticsDto
            {
                TeamId = teamId,
                CompetitionId = competitionId,
                Scope = scope,
                TournamentGroupId = tournamentGroupId,
                TeamName = names.TryGetValue(teamId, out string? name) ? name : string.Empty
            });
        }

        return merged;
    }

    public static List<HockeyTeamCompetitionStatisticsDto> DistinctStandings(
        IEnumerable<HockeyTeamCompetitionStatisticsDto> rows)
    {
        return rows
            .GroupBy(row => row.TeamId)
            .Select(group => group
                .OrderBy(row => row.StandingRank)
                .ThenByDescending(row => row.Points)
                .First())
            .OrderBy(row => row.StandingRank)
            .ThenByDescending(row => row.Points)
            .ToList();
    }
}
