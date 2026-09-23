using Application.Features.Football.Teams.DTOs;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;

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
        CancellationToken cancellationToken)
    {
        FootballCompetition? competition = await competitions.GetByIdAsync(competitionId);
        if (competition is null || !IsStarted(competition))
            return existing.ToList();

        List<FootballTeam> enrolled = await LoadEnrolledTeamsAsync(competition, tournaments, teams, cancellationToken);
        return Merge(existing, enrolled, competition.Id, competition.Name ?? string.Empty);
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

        return merged
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.GoalDifference)
            .ThenByDescending(row => row.GoalsFor)
            .ThenBy(row => row.TeamName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
