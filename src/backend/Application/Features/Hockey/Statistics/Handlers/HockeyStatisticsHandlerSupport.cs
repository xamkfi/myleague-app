using Application.Features.Hockey.Statistics.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;

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

    public static void AssignStandingRanks(IList<Domain.Entities.Hockey.Statistics.HockeyTeamCompetitionStatistics> teams)
    {
        List<Domain.Entities.Hockey.Statistics.HockeyTeamCompetitionStatistics> ordered = teams
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.RegulationWins)
            .ThenByDescending(t => t.GoalDifference)
            .ThenByDescending(t => t.GoalsFor)
            .ToList();

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

        return MergeZeros(existing, enrolledTeamIds, names, competitionId, scope, tournamentGroupId);
    }

    private static List<HockeyTeamCompetitionStatisticsDto> MergeZeros(
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

        List<HockeyTeamCompetitionStatisticsDto> ordered = merged
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.RegulationWins)
            .ThenByDescending(row => row.GoalDifference)
            .ThenByDescending(row => row.GoalsFor)
            .ThenBy(row => row.TeamName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (int index = 0; index < ordered.Count; index++)
            ordered[index].StandingRank = index + 1;

        return ordered;
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
