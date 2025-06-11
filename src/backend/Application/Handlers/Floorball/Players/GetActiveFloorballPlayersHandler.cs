using Application.Queries.Floorball.Player;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Domain.Enums.Floorball;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for retrieving paginated active floorball players with filtering support
/// </summary>
public class GetActiveFloorballPlayersHandler : BasePagedQueryHandler<GetActiveFloorballPlayersQuery, FloorballPlayerDto>,
    IRequestHandler<GetActiveFloorballPlayersQuery, Result<PagedResult<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;

    /// <summary>
    /// Initializes a new instance of the GetActiveFloorballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetActiveFloorballPlayersHandler(
        IFloorballPlayerRepository playerRepository,
        IPaginationService paginationService,
        ILogger<GetActiveFloorballPlayersHandler> logger) : base(paginationService, logger)
    {
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Handles the GetActiveFloorballPlayersQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of active floorball players as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FloorballPlayerDto>>> Handle(GetActiveFloorballPlayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving active floorball players - Page: {Page}, PageSize: {PageSize}, Position: {Position}, TeamId: {TeamId}", 
                request.Page, request.PageSize, request.Position, request.TeamId);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetActiveFloorballPlayersQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FloorballPlayerDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get all players and filter for active ones (will be moved to repository level)
            IEnumerable<FloorballPlayer> allPlayers = await _playerRepository.GetAllAsync();
            
            // Apply active filter first (this is the core purpose of this query)
            IEnumerable<FloorballPlayer> activePlayers = allPlayers.Where(p => p.IsActive);
            
            // Apply additional filtering
            if (!string.IsNullOrEmpty(request.Position))
            {
                if (Enum.TryParse<FloorballPosition>(request.Position, true, out FloorballPosition position))
                {
                    activePlayers = activePlayers.Where(p => p.Position.CanPlayInPosition(position));
                }
            }
            
            // TODO: Implement team filtering when player-team relationship is established
            if (request.TeamId.HasValue)
            {
                // For now, skip team filtering as the relationship structure needs to be clarified
                _logger.LogWarning("Team filtering requested but not yet implemented for FloorballPlayer entity");
            }

            // Apply pagination in memory (this will be moved to repository level)
            int totalCount = activePlayers.Count();
            IEnumerable<FloorballPlayer> players = activePlayers
                .Skip((request.Page - 1) * actualPageSize)
                .Take(actualPageSize);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<FloorballPlayerDto> playerDtos = FloorballPlayerMapper.ToDtos(players);
            
            PagedResult<FloorballPlayerDto> pagedResult = CreatePagedResult(
                playerDtos, 
                totalCount, 
                request.Page, 
                actualPageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} active floorball players out of {TotalCount} total", 
                players.Count(), totalCount);

            return Result<PagedResult<FloorballPlayerDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Active floorball players retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active floorball players");
            return Result<PagedResult<FloorballPlayerDto>>.Failure("An error occurred while retrieving active floorball players.");
        }
    }
} 