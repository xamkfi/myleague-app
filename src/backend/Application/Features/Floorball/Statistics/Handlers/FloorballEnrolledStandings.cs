using Application.Features.Floorball.Teams.DTOs;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Fills public standings with zero rows for teams enrolled in a started competition.
/// </summary>
internal static class FloorballEnrolledStandings
{
    public static bool IsStarted(FloorballCompetition competition)
    {
        if (competition is FloorballTournament tournament)
        {
            return tournament.TournamentStatus is FloorballTournamentStatus.GroupStage
                or FloorballTournamentStatus.PlayoffStage
                or FloorballTournamentStatus.Completed;
        }

        return competition.IsActive || competition.IsCompleted;
    }

    public static async Task<List<FloorballTeamSeasonStatisticsDto>> WithEnrolledZerosAsync(
        Guid competitionId,
        IReadOnlyList<FloorballTeamSeasonStatisticsDto> existing,
        IFloorballCompetitionRepository competitions,
        IFloorballTournamentRepository tournaments,
        IFloorballTeamRepository teams,
        CancellationToken cancellationToken)
    {
        FloorballCompetition? competition = await competitions.GetByIdAsync(competitionId);
        if (competition is null || !IsStarted(competition))
            return existing.ToList();

        List<FloorballTeam> enrolled = await LoadEnrolledTeamsAsync(competition, tournaments, teams, cancellationToken);
        return Merge(existing, enrolled, competition.Id, competition.Name ?? string.Empty);
    }

    private static async Task<List<FloorballTeam>> LoadEnrolledTeamsAsync(
        FloorballCompetition competition,
        IFloorballTournamentRepository tournaments,
        IFloorballTeamRepository teams,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, FloorballTeam> enrolled = new();
        foreach (FloorballTeam team in competition.Teams)
            enrolled[team.Id] = team;

        if (competition is not FloorballTournament)
            return enrolled.Values.ToList();

        FloorballTournament? withGroups = await tournaments.GetByIdWithGroupsAsNoTrackingAsync(competition.Id, cancellationToken);
        if (withGroups is null)
            return enrolled.Values.ToList();

        foreach (FloorballTournamentGroup group in withGroups.Groups)
        {
            foreach (FloorballTournamentGroupTeam membership in group.Teams)
            {
                if (membership.Team is not null)
                {
                    enrolled[membership.Team.Id] = membership.Team;
                    continue;
                }

                if (enrolled.ContainsKey(membership.TeamId))
                    continue;

                FloorballTeam? loaded = await teams.GetByIdAsync(membership.TeamId);
                if (loaded is not null)
                    enrolled[loaded.Id] = loaded;
            }
        }

        return enrolled.Values.ToList();
    }

    private static List<FloorballTeamSeasonStatisticsDto> Merge(
        IReadOnlyList<FloorballTeamSeasonStatisticsDto> existing,
        IReadOnlyList<FloorballTeam> enrolled,
        Guid competitionId,
        string seasonName)
    {
        HashSet<Guid> present = existing.Select(row => row.TeamId).ToHashSet();
        List<FloorballTeamSeasonStatisticsDto> merged = existing.ToList();
        foreach (FloorballTeam team in enrolled.Where(team => present.Add(team.Id)))
        {
            merged.Add(new FloorballTeamSeasonStatisticsDto
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
