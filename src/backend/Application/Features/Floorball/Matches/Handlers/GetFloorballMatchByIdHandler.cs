using Application.Features.Floorball.Players.Queries;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Floorball.Matches.Queries;
using Microsoft.EntityFrameworkCore;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for retrieving a floorball match by ID
/// </summary>
public class GetFloorballMatchByIdHandler : IRequestHandler<GetFloorballMatchByIdQuery, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly ILogger<GetFloorballMatchByIdHandler> _logger;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IClubRepository _clubRepository;

    /// <summary>
    /// Initializes a new instance of the GetFloorballMatchByIdHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="logger">The logger</param>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="clubRepository">The club repository</param>
    public GetFloorballMatchByIdHandler(
        IFloorballMatchRepository matchRepository,
        ILogger<GetFloorballMatchByIdHandler> logger,
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IClubRepository clubRepository)
    {
        _matchRepository = matchRepository;
        _logger = logger;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _clubRepository = clubRepository;
    }

    /// <summary>
    /// Handles the GetFloorballMatchByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The match as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(GetFloorballMatchByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball match with ID: {MatchId}", request.Id);
            
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found", request.Id);
                return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.Id);
            }

            // Get all unique player IDs from goal events and penalty events
            IEnumerable<Guid> playerIds = match.GoalEvents
                .SelectMany(g => new[] { g.ScoringPlayerId, g.AssistingPlayerId, g.SecondaryAssistingPlayerId })
                .Concat(match.PenaltyEvents.Select(p => p.PlayerId))
                .OfType<Guid>()
                .Distinct()
                .ToList();

            // Load players and their person data
            Dictionary<Guid, Person> playerPersonLookup = new Dictionary<Guid, Person>();
            if (playerIds.Any())
            {
                // Get players to map player ID to person ID
                List<FloorballPlayer> players = new List<FloorballPlayer>();
                foreach (Guid playerId in playerIds)
                {
                    FloorballPlayer? player = await _playerRepository.GetByIdAsync(playerId);
                    if (player != null)
                    {
                        players.Add(player);
                    }
                }

                // Extract person IDs from players
                List<Guid> personIds = players.Select(p => p.PersonId).Distinct().ToList();
                
                // Load persons using PersonRepository
                if (personIds.Any())
                {
                    IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);
                    Dictionary<Guid, Person> personLookup = persons.ToDictionary(p => p.Id, p => p);
                    
                    // Create lookup from player ID to person
                    foreach (FloorballPlayer player in players)
                    {
                        if (personLookup.TryGetValue(player.PersonId, out Person? person))
                        {
                            playerPersonLookup[player.Id] = person;
                        }
                    }
                }
            }

            // Load clubs for logo resolution (cross-context)
            List<Guid> clubIds = new Guid?[] { match.HomeTeam?.ClubId, match.AwayTeam?.ClubId }
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            Dictionary<Guid, Club> clubLookup = await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            Club? homeClub = match.HomeTeam != null && clubLookup.TryGetValue(match.HomeTeam.ClubId, out Club? resolvedHomeClub)
                ? resolvedHomeClub
                : null;
            Club? awayClub = match.AwayTeam != null && clubLookup.TryGetValue(match.AwayTeam.ClubId, out Club? resolvedAwayClub)
                ? resolvedAwayClub
                : null;

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match, playerPersonLookup, homeClub, awayClub);
            _logger.LogInformation("Successfully retrieved floorball match: {MatchId}", match.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while retrieving the floorball match.");
        }
    }
} 
