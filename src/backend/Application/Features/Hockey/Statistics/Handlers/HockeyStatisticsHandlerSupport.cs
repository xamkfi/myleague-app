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
}
