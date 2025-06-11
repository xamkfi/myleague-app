using Application.Queries.Floorball;
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
using Application.Queries.Floorball.Player;
using System.Linq;
using Domain.Enums.Floorball;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for retrieving paginated floorball players with comprehensive filtering support
/// </summary>
public class GetAllFloorballPlayersHandler : BasePagedQueryHandler<GetAllFloorballPlayersQuery, FloorballPlayerDto>,
    IRequestHandler<GetAllFloorballPlayersQuery, Result<PagedResult<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballPlayersHandler(
        IFloorballPlayerRepository playerRepository,
        IPaginationService paginationService,
        ILogger<GetAllFloorballPlayersHandler> logger) : base(paginationService, logger)
    {
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Handles the GetAllFloorballPlayersQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of floorball players as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FloorballPlayerDto>>> Handle(GetAllFloorballPlayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving floorball players - Page: {Page}, PageSize: {PageSize}, IsActive: {IsActive}, Position: {Position}, TeamId: {TeamId}, SearchTerm: {SearchTerm}", 
                request.Page, request.PageSize, request.IsActive, request.Position, request.TeamId, request.SearchTerm);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllFloorballPlayersQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FloorballPlayerDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get all players and apply comprehensive filtering (will be moved to repository level)
            IEnumerable<FloorballPlayer> allPlayers = await _playerRepository.GetAllAsync();
            
            // Apply filtering
            IEnumerable<FloorballPlayer> filteredPlayers = allPlayers;
            
            // Apply active status filter
            if (request.IsActive.HasValue)
            {
                filteredPlayers = filteredPlayers.Where(p => p.IsActive == request.IsActive.Value);
            }
            
            // Apply position filter
            if (!string.IsNullOrEmpty(request.Position))
            {
                if (Enum.TryParse<FloorballPosition>(request.Position, true, out FloorballPosition position))
                {
                    filteredPlayers = filteredPlayers.Where(p => p.Position.CanPlayInPosition(position));
                }
            }
            
            // Apply team filter
            // TODO: Implement team filtering when player-team relationship is established
            if (request.TeamId.HasValue)
            {
                // For now, skip team filtering as the relationship structure needs to be clarified
                _logger.LogWarning("Team filtering requested but not yet implemented for FloorballPlayer entity");
            }
            
            // Apply search term filter (search in person's name - this will need to be handled at repository level)
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                string searchLower = request.SearchTerm.ToLower();
                filteredPlayers = filteredPlayers.Where(p => 
                    p.Person.FirstName.ToLower().Contains(searchLower) ||
                    p.Person.LastName.ToLower().Contains(searchLower) ||
                    $"{p.Person.FirstName} {p.Person.LastName}".ToLower().Contains(searchLower));
            }

            // Apply pagination in memory (this will be moved to repository level)
            int totalCount = filteredPlayers.Count();
            IEnumerable<FloorballPlayer> players = filteredPlayers
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
            
            _logger.LogInformation("Successfully retrieved {Count} floorball players out of {TotalCount} total", 
                players.Count(), totalCount);

            return Result<PagedResult<FloorballPlayerDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Floorball players retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball players");
            return Result<PagedResult<FloorballPlayerDto>>.Failure("An error occurred while retrieving floorball players.");
        }
    }
} 
