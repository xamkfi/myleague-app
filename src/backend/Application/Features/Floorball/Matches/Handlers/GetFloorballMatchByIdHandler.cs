using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Floorball.Match;
using Microsoft.EntityFrameworkCore;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace Application.Handlers.Floorball.Matches;

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
            List<Guid> clubIds = new List<Guid> { match.HomeTeam.ClubId, match.AwayTeam.ClubId }
                .Distinct()
                .ToList();
            Dictionary<Guid, Club> clubLookup = await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            clubLookup.TryGetValue(match.HomeTeam.ClubId, out Club? homeClub);
            clubLookup.TryGetValue(match.AwayTeam.ClubId, out Club? awayClub);

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
