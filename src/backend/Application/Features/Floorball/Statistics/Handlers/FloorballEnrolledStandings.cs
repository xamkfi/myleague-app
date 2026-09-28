using Application.Common;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Services.Common;

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
        IClubRepository clubs,
        IFloorballMatchRepository matches,
        CancellationToken cancellationToken)
    {
        FloorballCompetition? competition = await competitions.GetByIdAsync(competitionId);
        List<FloorballTeam> knownTeams = new();
        List<FloorballTeamSeasonStatisticsDto> rows;
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
        IReadOnlyList<StandingMatchResult> played = await LoadResultsAsync(competitionId, criteria, matches);
        return StandingTableOrder.Sort(
            rows,
            criteria,
            row => new StandingSortSnapshot(
                row.TeamId,
                row.Points,
                row.GoalDifference,
                row.GoalsFor,
                row.GoalsAgainst,
                row.PenaltyMinutes,
                row.TeamName),
            played);
    }

    private static async Task<IReadOnlyList<StandingMatchResult>> LoadResultsAsync(
        Guid competitionId,
        IReadOnlyList<StandingSortCriterion> criteria,
        IFloorballMatchRepository matches)
    {
        if (!StandingSortCriteria.UsesHeadToHead(criteria))
            return [];

        List<StandingMatchResult> results = new();
        IEnumerable<FloorballMatch> validMatches = (await matches.GetByCompetitionIdAsync(competitionId))
            .Where(match => match.Status == FloorballMatchStatus.Completed
                && match.PlayoffRound is null
                && match.HomeTeamId is Guid homeId && homeId != Guid.Empty
                && match.AwayTeamId is Guid awayId && awayId != Guid.Empty);
        foreach (FloorballMatch match in validMatches)
        {
            Guid homeId = match.HomeTeamId!.Value;
            Guid awayId = match.AwayTeamId!.Value;
            int homePoints = match.HomeScore > match.AwayScore ? 3 : match.HomeScore == match.AwayScore ? 1 : 0;
            int awayPoints = match.AwayScore > match.HomeScore ? 3 : match.HomeScore == match.AwayScore ? 1 : 0;
            results.Add(new StandingMatchResult(homeId, awayId, match.HomeScore, match.AwayScore, homePoints, awayPoints));
        }

        return results;
    }

    private static IReadOnlyList<StandingSortCriterion> CriteriaFor(FloorballCompetition? competition)
    {
        if (competition is FloorballSeason season && season.RankingCriteria.Count > 0)
            return season.RankingCriteria;

        return StandingSortCriteria.LegacyWithoutGoalsAgainst;
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

        return merged;
    }

    private static async Task ApplyMarksAsync(
        List<FloorballTeamSeasonStatisticsDto> rows,
        IReadOnlyList<FloorballTeam> knownTeams,
        IFloorballTeamRepository teams,
        IClubRepository clubs,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return;

        Dictionary<Guid, FloorballTeam> byId = new();
        foreach (FloorballTeam team in knownTeams)
            byId[team.Id] = team;

        foreach (Guid teamId in rows.Select(row => row.TeamId).Distinct().Where(teamId => !byId.ContainsKey(teamId)))
        {
            FloorballTeam? loaded = await teams.GetByIdAsync(teamId);
            if (loaded is not null)
                byId[loaded.Id] = loaded;
        }

        List<Guid> clubIds = byId.Values.Select(team => team.ClubId).Distinct().ToList();
        Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
            ? new Dictionary<Guid, Club>()
            : await clubs.GetByIdsAsync(clubIds, cancellationToken);

        foreach (FloorballTeamSeasonStatisticsDto row in rows.Where(row => byId.ContainsKey(row.TeamId)))
        {
            FloorballTeam team = byId[row.TeamId];
            clubLookup.TryGetValue(team.ClubId, out Club? club);
            row.TeamLogo = PublicLogoUrl.OmitPlaceholder(team.GetEffectiveLogoUrl(club?.LogoUrl));
            row.TeamShortName = team.ShortName;
        }
    }
}
