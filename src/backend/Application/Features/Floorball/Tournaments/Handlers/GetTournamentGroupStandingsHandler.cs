using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Queries;
using Domain.Entities.Floorball;
using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Handler for calculating and retrieving group standings for a specific tournament group.
/// Computes wins, draws, losses, goals, goal difference, and points from completed matches.
/// </summary>
public class GetTournamentGroupStandingsHandler
    : IRequestHandler<GetTournamentGroupStandingsQuery, Result<FloorballTournamentGroupStandingsDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly ILogger<GetTournamentGroupStandingsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTournamentGroupStandingsHandler class
    /// </summary>
    /// <param name="tournamentRepository">The floorball tournament repository</param>
    /// <param name="logger">The logger</param>
    public GetTournamentGroupStandingsHandler(
        IFloorballTournamentRepository tournamentRepository,
        ILogger<GetTournamentGroupStandingsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTournamentGroupStandingsQuery request
    /// </summary>
    /// <param name="request">The query containing tournament and group IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The group standings DTO wrapped in a Result</returns>
    public async Task<Result<FloorballTournamentGroupStandingsDto>> Handle(
        GetTournamentGroupStandingsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Calculating group standings for Tournament: {TournamentId}, Group: {GroupId}",
                request.TournamentId, request.GroupId);

            FloorballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.TournamentId);
            if (tournament == null)
            {
                _logger.LogWarning("Floorball tournament with ID {TournamentId} not found", request.TournamentId);
                return Result<FloorballTournamentGroupStandingsDto>.NotFound("FloorballTournament", request.TournamentId);
            }

            FloorballTournamentGroup? group = tournament.Groups.FirstOrDefault(g => g.Id == request.GroupId);
            if (group == null)
            {
                _logger.LogWarning(
                    "Group {GroupId} not found in tournament {TournamentId}",
                    request.GroupId, request.TournamentId);
                return Result<FloorballTournamentGroupStandingsDto>.NotFound("FloorballTournamentGroup", request.GroupId);
            }

            List<FloorballMatch> completedMatches = tournament.Matches
                .Where(m => m.TournamentGroupId == request.GroupId && m.Status == FloorballMatchStatus.Completed)
                .ToList();

            List<FloorballTournamentGroupStandingEntryDto> entries = new();

            foreach (FloorballTournamentGroupTeam groupTeam in group.Teams)
            {
                Guid teamId = groupTeam.TeamId;

                List<FloorballMatch> teamMatches = completedMatches
                    .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                    .ToList();

                int gamesPlayed = teamMatches.Count;
                int wins = 0;
                int draws = 0;
                int losses = 0;
                int goalsFor = 0;
                int goalsAgainst = 0;

                foreach (FloorballMatch match in teamMatches)
                {
                    bool isHome = match.HomeTeamId == teamId;
                    int teamGoals = isHome ? match.HomeScore : match.AwayScore;
                    int opponentGoals = isHome ? match.AwayScore : match.HomeScore;

                    goalsFor += teamGoals;
                    goalsAgainst += opponentGoals;

                    if (teamGoals > opponentGoals)
                        wins++;
                    else if (teamGoals == opponentGoals)
                        draws++;
                    else
                        losses++;
                }

                int points = (wins * 3) + (draws * 1);
                int goalDifference = goalsFor - goalsAgainst;

                entries.Add(new FloorballTournamentGroupStandingEntryDto(
                    Rank: 0,
                    TeamId: teamId,
                    TeamName: groupTeam.Team?.Name ?? "Unknown Team",
                    GamesPlayed: gamesPlayed,
                    Wins: wins,
                    Draws: draws,
                    Losses: losses,
                    GoalsFor: goalsFor,
                    GoalsAgainst: goalsAgainst,
                    GoalDifference: goalDifference,
                    Points: points));
            }

            List<FloorballTournamentGroupStandingEntryDto> rankedEntries = entries
                .OrderByDescending(e => e.Points)
                .ThenByDescending(e => e.GoalDifference)
                .ThenByDescending(e => e.GoalsFor)
                .Select((entry, index) => entry with { Rank = index + 1 })
                .ToList();

            FloorballTournamentGroupStandingsDto standings = new(
                GroupId: group.Id,
                GroupName: group.Name,
                Entries: rankedEntries.AsReadOnly());

            _logger.LogInformation(
                "Successfully calculated standings for Group '{GroupName}' in Tournament {TournamentId} - {TeamCount} teams",
                group.Name, request.TournamentId, rankedEntries.Count);

            return Result<FloorballTournamentGroupStandingsDto>.Success(standings);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while calculating group standings for Tournament: {TournamentId}, Group: {GroupId}",
                request.TournamentId, request.GroupId);
            return Result<FloorballTournamentGroupStandingsDto>.Failure(
                "An error occurred while calculating group standings.");
        }
    }
}
