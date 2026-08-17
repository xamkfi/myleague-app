using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Players.Queries;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Matches;
using Domain.Entities.Common;
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Domain.Enums.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Players.Handlers
{
    /// <summary>
    /// Handler for retrieving a football player's match history with performance statistics
    /// </summary>
    public class GetFootballPlayerMatchesHandler : IRequestHandler<GetFootballPlayerMatchesQuery, Result<FootballPlayerWithMatchesDto>>
    {
        private readonly IFootballPlayerRepository _playerRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IFootballTeamRepository _teamRepository;
        private readonly IFootballMatchRepository _matchRepository;
        private readonly ILogger<GetFootballPlayerMatchesHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetFootballPlayerMatchesHandler class
        /// </summary>
        public GetFootballPlayerMatchesHandler(
            IFootballPlayerRepository playerRepository,
            IPersonRepository personRepository,
            IFootballTeamRepository teamRepository,
            IFootballMatchRepository matchRepository,
            ILogger<GetFootballPlayerMatchesHandler> logger)
        {
            _playerRepository = playerRepository;
            _personRepository = personRepository;
            _teamRepository = teamRepository;
            _matchRepository = matchRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetFootballPlayerMatchesQuery request
        /// </summary>
        public async Task<Result<FootballPlayerWithMatchesDto>> Handle(GetFootballPlayerMatchesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving match history for player: {PlayerId}", request.PlayerId);

                // Get the player
                FootballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found", request.PlayerId);
                    return Result<FootballPlayerWithMatchesDto>.NotFound("FootballPlayer", request.PlayerId);
                }

                // Get the associated person
                Person? person = await _personRepository.GetByIdAsync(player.PersonId);
                if (person == null)
                {
                    _logger.LogWarning("Person with ID {PersonId} not found for player {PlayerId}", player.PersonId, player.Id);
                    return Result<FootballPlayerWithMatchesDto>.Failure("Associated person not found");
                }

                // Get all teams the player has been part of
                IEnumerable<FootballTeam> playerTeamsEnumerable = await _teamRepository.GetByPlayerIdAsync(player.Id);
                List<FootballTeam> playerTeams = playerTeamsEnumerable.ToList();

                if (!playerTeams.Any())
                {
                    _logger.LogInformation("Player {PlayerId} has no team associations", request.PlayerId);
                    
                    // Return player info with no matches
                    FootballPlayerWithMatchesDto emptyResult = new FootballPlayerWithMatchesDto(
                        player.Id,
                        person.FullName,
                        player.Position.PrimaryPosition,
                        null,
                        "No Team",
                        Guid.Empty,
                        player.IsActive,
                        new List<FootballPlayerTeamCareerStatsDto>(),
                        new List<FootballPlayerMatchDto>()
                    );
                    
                    return Result<FootballPlayerWithMatchesDto>.Success(emptyResult);
                }

                // Get the most recent team (assuming the player's current team)
                FootballTeam currentTeam = playerTeams.OrderByDescending(t => t.CreatedAt).First();
                FootballTeamPlayer? teamPlayer = currentTeam.Roster.FirstOrDefault(r => r.PlayerId == player.Id);

                // Get all matches for all teams the player has been part of
                List<FootballMatch> allMatches = new List<FootballMatch>();
                foreach (FootballTeam team in playerTeams)
                {
                    IEnumerable<FootballMatch> teamMatches = await _matchRepository.GetByTeamIdAsync(team.Id);
                    allMatches.AddRange(teamMatches.Where(m => m.Status == FootballMatchStatus.Completed));
                }

                // Sort matches by date (most recent first) and take the requested limit
                List<FootballMatch> recentMatches = allMatches
                    .OrderByDescending(m => m.ScheduledDateTime)
                    .Take(request.Limit)
                    .ToList();

                // Build the response
                List<FootballPlayerMatchDto> playerMatchDtos = new List<FootballPlayerMatchDto>();

                foreach (FootballMatch match in recentMatches)
                {
                    // Get the team this player was playing for in this match
                    FootballTeam? playerTeamInMatch = playerTeams.FirstOrDefault(t => 
                        t.Id == match.HomeTeamId || t.Id == match.AwayTeamId);

                    if (playerTeamInMatch == null) continue;

                    FootballTeamPlayer? playerInMatchTeam = playerTeamInMatch.Roster
                        .FirstOrDefault(r => r.PlayerId == player.Id);

                    // Calculate player stats for this match
                    // Note: This is simplified - in a real implementation, you'd need to track 
                    // per-match statistics separately
                    FootballPlayerMatchStatsDto playerStats = new FootballPlayerMatchStatsDto(
                        Goals: CalculatePlayerGoalsInMatch(match, player.Id),
                        Assists: CalculatePlayerAssistsInMatch(match, player.Id),
                        YellowCards: CalculatePlayerYellowCardsInMatch(match, player.Id),
                        RedCards: CalculatePlayerRedCardsInMatch(match, player.Id),
                        PlayedMinutes: 90 // Default to full match - would need separate tracking
                    );

                    // Convert period scores
                    Dictionary<int, (int HomeScore, int AwayScore)> periodScores = match.PeriodScores.ToDictionary(
                        ps => ps.PeriodNumber,
                        ps => (ps.HomeScore, ps.AwayScore)
                    );

                    FootballPlayerMatchDto matchDto = new FootballPlayerMatchDto(
                        match.Id,
                        match.CompetitionId,
                        match.Competition?.Name ?? "",
                        match.HomeTeamId,
                        match.HomeTeam?.Name ?? "Unknown Team",
                        match.AwayTeamId,
                        match.AwayTeam?.Name ?? "Unknown Team",
                        match.ScheduledDateTime,
                        match.Venue,
                        match.Status,
                        match.HomeScore,
                        match.AwayScore,
                        match.WentToExtraTime,
                        match.WentToPenaltyShootout,
                        periodScores,
                        playerStats
                    );

                    playerMatchDtos.Add(matchDto);
                }

                // Calculate team-specific career stats
                List<FootballPlayerTeamCareerStatsDto> teamCareerStats = new List<FootballPlayerTeamCareerStatsDto>();
                
                foreach (FootballTeam team in playerTeams)
                {
                    FootballTeamPlayer? playerInTeam = team.Roster.FirstOrDefault(r => r.PlayerId == player.Id);
                    if (playerInTeam != null)
                    {
                        FootballPlayerStatsDto teamStats = new FootballPlayerStatsDto(
                            playerInTeam.GamesPlayed,
                            playerInTeam.Goals,
                            playerInTeam.Assists,
                            playerInTeam.Goals + playerInTeam.Assists,
                            playerInTeam.YellowCards,
                            playerInTeam.RedCards
                        );

                        FootballPlayerTeamCareerStatsDto teamCareerStat = new FootballPlayerTeamCareerStatsDto(
                            team.Id,
                            team.Name,
                            teamStats
                        );

                        teamCareerStats.Add(teamCareerStat);
                    }
                }

                FootballPlayerWithMatchesDto result = new FootballPlayerWithMatchesDto(
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

                return Result<FootballPlayerWithMatchesDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving match history for player: {PlayerId}", request.PlayerId);
                return Result<FootballPlayerWithMatchesDto>.Failure($"Error retrieving player match history: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate goals scored by a player in a specific match
        /// </summary>
        private static int CalculatePlayerGoalsInMatch(FootballMatch match, Guid playerId)
        {
            return match.GoalEvents.Count(g => g.ScoringPlayerId == playerId);
        }

        /// <summary>
        /// Calculate assists made by a player in a specific match
        /// </summary>
        private static int CalculatePlayerAssistsInMatch(FootballMatch match, Guid playerId)
        {
            return match.GoalEvents.Count(g => g.AssistingPlayerId == playerId);
        }

        /// <summary>
        /// Calculate yellow cards shown to a player in a specific match
        /// </summary>
        private static int CalculatePlayerYellowCardsInMatch(FootballMatch match, Guid playerId)
        {
            return match.CardEvents.Count(c => c.PlayerId == playerId && c.CardType == FootballCardType.Yellow);
        }

        /// <summary>
        /// Calculate sending-off cards shown to a player in a specific match
        /// </summary>
        private static int CalculatePlayerRedCardsInMatch(FootballMatch match, Guid playerId)
        {
            return match.CardEvents.Count(c =>
                c.PlayerId == playerId &&
                (c.CardType == FootballCardType.DirectRed || c.CardType == FootballCardType.SecondYellow));
        }
    }
} 
