using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Players.Queries;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Players.Handlers
{
    /// <summary>
    /// Handler for retrieving a floorball player's match history with performance statistics
    /// </summary>
    public class GetFloorballPlayerMatchesHandler : IRequestHandler<GetFloorballPlayerMatchesQuery, Result<FloorballPlayerWithMatchesDto>>
    {
        private readonly IFloorballPlayerRepository _playerRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IFloorballTeamRepository _teamRepository;
        private readonly IFloorballMatchRepository _matchRepository;
        private readonly ILogger<GetFloorballPlayerMatchesHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetFloorballPlayerMatchesHandler class
        /// </summary>
        public GetFloorballPlayerMatchesHandler(
            IFloorballPlayerRepository playerRepository,
            IPersonRepository personRepository,
            IFloorballTeamRepository teamRepository,
            IFloorballMatchRepository matchRepository,
            ILogger<GetFloorballPlayerMatchesHandler> logger)
        {
            _playerRepository = playerRepository;
            _personRepository = personRepository;
            _teamRepository = teamRepository;
            _matchRepository = matchRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetFloorballPlayerMatchesQuery request
        /// </summary>
        public async Task<Result<FloorballPlayerWithMatchesDto>> Handle(GetFloorballPlayerMatchesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving match history for player: {PlayerId}", request.PlayerId);

                // Get the player
                FloorballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found", request.PlayerId);
                    return Result<FloorballPlayerWithMatchesDto>.NotFound("FloorballPlayer", request.PlayerId);
                }

                // Get the associated person
                Person? person = await _personRepository.GetByIdAsync(player.PersonId);
                if (person == null)
                {
                    _logger.LogWarning("Person with ID {PersonId} not found for player {PlayerId}", player.PersonId, player.Id);
                    return Result<FloorballPlayerWithMatchesDto>.Failure("Associated person not found");
                }

                // Get all teams the player has been part of
                IEnumerable<FloorballTeam> playerTeamsEnumerable = await _teamRepository.GetByPlayerIdAsync(player.Id);
                List<FloorballTeam> playerTeams = playerTeamsEnumerable.ToList();

                if (!playerTeams.Any())
                {
                    _logger.LogInformation("Player {PlayerId} has no team associations", request.PlayerId);
                    
                    // Return player info with no matches
                    FloorballPlayerWithMatchesDto emptyResult = new FloorballPlayerWithMatchesDto(
                        player.Id,
                        person.FullName,
                        player.Position.PrimaryPosition,
                        null,
                        "No Team",
                        Guid.Empty,
                        player.IsActive,
                        new List<FloorballPlayerTeamCareerStatsDto>(),
                        new List<FloorballPlayerMatchDto>()
                    );
                    
                    return Result<FloorballPlayerWithMatchesDto>.Success(emptyResult);
                }

                // Get the most recent team (assuming the player's current team)
                FloorballTeam currentTeam = playerTeams.OrderByDescending(t => t.CreatedAt).First();
                FloorballTeamPlayer? teamPlayer = currentTeam.Roster.FirstOrDefault(r => r.PlayerId == player.Id);

                // Get all matches for all teams the player has been part of
                List<FloorballMatch> allMatches = new List<FloorballMatch>();
                foreach (FloorballTeam team in playerTeams)
                {
                    IEnumerable<FloorballMatch> teamMatches = await _matchRepository.GetByTeamIdAsync(team.Id);
                    allMatches.AddRange(teamMatches.Where(m => m.Status == FloorballMatchStatus.Completed));
                }

                // Sort matches by date (most recent first) and take the requested limit
                List<FloorballMatch> recentMatches = allMatches
                    .OrderByDescending(m => m.ScheduledDateTime)
                    .Take(request.Limit)
                    .ToList();

                // Build the response
                List<FloorballPlayerMatchDto> playerMatchDtos = new List<FloorballPlayerMatchDto>();

                foreach (FloorballMatch match in recentMatches)
                {
                    // Get the team this player was playing for in this match
                    FloorballTeam? playerTeamInMatch = playerTeams.FirstOrDefault(t => 
                        t.Id == match.HomeTeamId || t.Id == match.AwayTeamId);

                    if (playerTeamInMatch == null) continue;

                    FloorballTeamPlayer? playerInMatchTeam = playerTeamInMatch.Roster
                        .FirstOrDefault(r => r.PlayerId == player.Id);

                    // Calculate player stats for this match
                    // Note: This is simplified - in a real implementation, you'd need to track 
                    // per-match statistics separately
                    FloorballPlayerMatchStatsDto playerStats = new FloorballPlayerMatchStatsDto(
                        Goals: CalculatePlayerGoalsInMatch(match, player.Id),
                        Assists: CalculatePlayerAssistsInMatch(match, player.Id),
                        PenaltyMinutes: CalculatePlayerPenaltiesInMatch(match, player.Id),
                        PlayedMinutes: 60 // Default to full match - would need separate tracking
                    );

                    // Convert period scores
                    Dictionary<int, (int HomeScore, int AwayScore)> periodScores = match.PeriodScores.ToDictionary(
                        ps => ps.PeriodNumber,
                        ps => (ps.HomeScore, ps.AwayScore)
                    );

                    FloorballPlayerMatchDto matchDto = new FloorballPlayerMatchDto(
                        match.Id,
                        match.SeasonId,
                        match.HomeTeamId,
                        match.HomeTeam?.Name ?? "Unknown Team",
                        match.AwayTeamId,
                        match.AwayTeam?.Name ?? "Unknown Team",
                        match.ScheduledDateTime,
                        match.Venue,
                        match.Status,
                        match.HomeScore,
                        match.AwayScore,
                        match.WentToOvertime,
                        match.WentToShootout,
                        periodScores,
                        playerStats
                    );

                    playerMatchDtos.Add(matchDto);
                }

                // Calculate team-specific career stats
                List<FloorballPlayerTeamCareerStatsDto> teamCareerStats = new List<FloorballPlayerTeamCareerStatsDto>();
                
                foreach (FloorballTeam team in playerTeams)
                {
                    FloorballTeamPlayer? playerInTeam = team.Roster.FirstOrDefault(r => r.PlayerId == player.Id);
                    if (playerInTeam != null)
                    {
                        FloorballPlayerStatsDto teamStats = new FloorballPlayerStatsDto(
                            playerInTeam.GamesPlayed,
                            playerInTeam.Goals,
                            playerInTeam.Assists,
                            playerInTeam.Goals + playerInTeam.Assists,
                            playerInTeam.PenaltyMinutes
                        );

                        FloorballPlayerTeamCareerStatsDto teamCareerStat = new FloorballPlayerTeamCareerStatsDto(
                            team.Id,
                            team.Name,
                            teamStats
                        );

                        teamCareerStats.Add(teamCareerStat);
                    }
                }

                FloorballPlayerWithMatchesDto result = new FloorballPlayerWithMatchesDto(
                    player.Id,
                    person.FullName,
                    player.Position.PrimaryPosition,
                    teamPlayer?.JerseyNumber,
                    currentTeam.Name,
                    currentTeam.Id,
                    player.IsActive,
                    teamCareerStats,
                    playerMatchDtos
                );

                _logger.LogInformation("Successfully retrieved match history for player: {PlayerId} with {MatchCount} matches", 
                    player.Id, playerMatchDtos.Count);

                return Result<FloorballPlayerWithMatchesDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving match history for player: {PlayerId}", request.PlayerId);
                return Result<FloorballPlayerWithMatchesDto>.Failure($"Error retrieving player match history: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate goals scored by a player in a specific match
        /// </summary>
        private static int CalculatePlayerGoalsInMatch(FloorballMatch match, Guid playerId)
        {
            return match.GoalEvents.Count(g => g.ScoringPlayerId == playerId);
        }

        /// <summary>
        /// Calculate assists made by a player in a specific match
        /// </summary>
        private static int CalculatePlayerAssistsInMatch(FloorballMatch match, Guid playerId)
        {
            return match.GoalEvents.Count(g => g.AssistingPlayerId == playerId);
        }

        /// <summary>
        /// Calculate penalty minutes for a player in a specific match
        /// </summary>
        private static int CalculatePlayerPenaltiesInMatch(FloorballMatch match, Guid playerId)
        {
            return match.PenaltyEvents
                .Where(p => p.PlayerId == playerId)
                .Sum(p => GetPenaltyMinutes(p.PenaltyType));
        }

        /// <summary>
        /// Get penalty minutes based on penalty type
        /// </summary>
        private static int GetPenaltyMinutes(FloorballPenaltyType penaltyType)
        {
            return penaltyType switch
            {
                FloorballPenaltyType.Minor => 2,
                FloorballPenaltyType.Major => 5,
                FloorballPenaltyType.Misconduct => 10,
                FloorballPenaltyType.MatchPenalty => 10,
                FloorballPenaltyType.Technical => 2,
                _ => 0
            };
        }
    }
} 
