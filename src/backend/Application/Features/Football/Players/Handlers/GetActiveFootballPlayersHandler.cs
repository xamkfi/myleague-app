using Application.Features.Football.Players.Queries;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Common;
using Domain.Common;
using Application.Services.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
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
using Domain.Enums.Football;

namespace Application.Features.Football.Players.Handlers;

/// <summary>
/// Handler for retrieving paginated active football players with filtering support
/// </summary>
public class GetActiveFootballPlayersHandler : BasePagedQueryHandler<GetActiveFootballPlayersQuery, FootballPlayerDto>,
    IRequestHandler<GetActiveFootballPlayersQuery, Result<PagedResult<FootballPlayerDto>>>
{
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetActiveFootballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetActiveFootballPlayersHandler(
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IPaginationService paginationService,
        ILogger<GetActiveFootballPlayersHandler> logger) : base(paginationService, logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
    }

    /// <summary>
    /// Handles the GetActiveFootballPlayersQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of active football players as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FootballPlayerDto>>> Handle(GetActiveFootballPlayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving active football players - Page: {Page}, PageSize: {PageSize}, Position: {Position}, TeamId: {TeamId}", 
                request.Page, request.PageSize, request.Position, request.TeamId);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetActiveFootballPlayersQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FootballPlayerDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Parse position filter if provided
            FootballPosition? positionFilter = null;
            if (!string.IsNullOrEmpty(request.Position))
            {
                if (Enum.TryParse<FootballPosition>(request.Position, true, out FootballPosition position))
                {
                    positionFilter = position;
                    _logger.LogDebug("Position filter applied: {Position}", position);
                }
                else
                {
                    _logger.LogWarning("Invalid position filter provided: {Position}", request.Position);
                    return Result<PagedResult<FootballPlayerDto>>.Failure($"Invalid position: {request.Position}");
                }
            }

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Use repository-level pagination with active filter
            PagedResult<FootballPlayer> pagedPlayers = await _playerRepository.GetPagedAsync(
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
            List<FootballPlayerDto> playerDtos = new List<FootballPlayerDto>();
            foreach (FootballPlayer player in pagedPlayers.Items)
            {
                // Get the associated person
                Person? person = await _personRepository.GetByIdAsync(player.PersonId);
                if (person != null)
                {
                    // Create DTO with real person data
                    FootballPlayerDto playerDto = new FootballPlayerDto(
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
                    playerDtos.Add(FootballPlayerMapper.ToDto(player));
                }
            }
            
            // Create the final paged result with DTOs
            PagedResult<FootballPlayerDto> pagedResult = CreatePagedResult(
                playerDtos, 
                pagedPlayers.TotalCount, 
                pagedPlayers.Page, 
                pagedPlayers.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} active football players out of {TotalCount} total", 
                pagedPlayers.ItemCount, pagedPlayers.TotalCount);

            return Result<PagedResult<FootballPlayerDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Active football players retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active football players");
            return Result<PagedResult<FootballPlayerDto>>.Failure("An error occurred while retrieving active football players.");
        }
    }
} 
