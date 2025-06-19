using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Application.Mappings.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Floorball.Player;
using Domain.Enums.Floorball;

namespace Application.Handlers.Floorball.Players;

/// <summary>
/// Handler for retrieving paginated floorball players with comprehensive filtering support
/// </summary>
public class GetAllFloorballPlayersHandler : BasePagedQueryHandler<GetAllFloorballPlayersQuery, FloorballPlayerDto>,
    IRequestHandler<GetAllFloorballPlayersQuery, Result<PagedResult<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballPlayersHandler(
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IPaginationService paginationService,
        ILogger<GetAllFloorballPlayersHandler> logger) : base(paginationService, logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
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

            // Parse position filter if provided
            FloorballPosition? positionFilter = null;
            if (!string.IsNullOrEmpty(request.Position))
            {
                if (Enum.TryParse<FloorballPosition>(request.Position, true, out FloorballPosition position))
                {
                    positionFilter = position;
                    _logger.LogDebug("Position filter applied: {Position}", position);
                }
                else
                {
                    _logger.LogWarning("Invalid position filter provided: {Position}", request.Position);
                    return Result<PagedResult<FloorballPlayerDto>>.Failure($"Invalid position: {request.Position}");
                }
            }

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Use repository-level pagination with all filters
            PagedResult<FloorballPlayer> pagedPlayers = await _playerRepository.GetPagedAsync(
                page: validationResult.Data.Page,
                pageSize: actualPageSize,
                isActive: request.IsActive,
                position: positionFilter,
                teamId: request.TeamId,
                searchTerm: request.SearchTerm,
                cancellationToken: cancellationToken);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Load Person data for each player
            List<FloorballPlayerDto> playerDtos = new List<FloorballPlayerDto>();
            foreach (FloorballPlayer player in pagedPlayers.Items)
            {
                // Get the associated person
                Person? person = await _personRepository.GetByIdAsync(player.PersonId);
                if (person != null)
                {
                    // Create DTO with real person data
                    FloorballPlayerDto playerDto = new FloorballPlayerDto(
                        player.Id,
                        player.PersonId,
                        PersonMapper.ToDto(person),
                        player.IsActive,
                        player.Position.PrimaryPosition,
                        player.CareerGoals,
                        player.CareerAssists
                    );
                    playerDtos.Add(playerDto);
                }
                else
                {
                    // Fallback to placeholder if person not found
                    playerDtos.Add(FloorballPlayerMapper.ToDto(player));
                }
            }
            
            // Create the final paged result with DTOs
            PagedResult<FloorballPlayerDto> pagedResult = CreatePagedResult(
                playerDtos, 
                pagedPlayers.TotalCount, 
                pagedPlayers.Page, 
                pagedPlayers.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball players out of {TotalCount} total", 
                pagedPlayers.ItemCount, pagedPlayers.TotalCount);

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
