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
using Domain.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Players.Handlers;

/// <summary>
/// Handler for retrieving paginated active floorball players with filtering support
/// </summary>
public class GetActiveFloorballPlayersHandler : BasePagedQueryHandler<GetActiveFloorballPlayersQuery, FloorballPlayerDto>,
    IRequestHandler<GetActiveFloorballPlayersQuery, Result<PagedResult<FloorballPlayerDto>>>
{
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetActiveFloorballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetActiveFloorballPlayersHandler(
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IPaginationService paginationService,
        ILogger<GetActiveFloorballPlayersHandler> logger) : base(paginationService, logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
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

            // Use repository-level pagination with active filter
            PagedResult<FloorballPlayer> pagedPlayers = await _playerRepository.GetPagedAsync(
                page: validationResult.Data.Page,
                pageSize: actualPageSize,
                isActive: true, // This is the key filter for active players
                position: positionFilter,
                teamId: request.TeamId,
                searchTerm: null, // Not used in this query
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
            
            _logger.LogInformation("Successfully retrieved {Count} active floorball players out of {TotalCount} total", 
                pagedPlayers.ItemCount, pagedPlayers.TotalCount);

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
