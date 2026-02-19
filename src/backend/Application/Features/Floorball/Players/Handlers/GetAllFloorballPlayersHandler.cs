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

            // Use repository-level pagination with all filters and team information
            PagedResult<(FloorballPlayer Player, FloorballTeam? Team)> pagedPlayersWithTeams = await _playerRepository.GetPagedWithTeamsAsync(
                page: validationResult.Data.Page,
                pageSize: actualPageSize,
                isActive: request.IsActive,
                position: positionFilter,
                teamId: request.TeamId,
                searchTerm: request.SearchTerm,
                cancellationToken: cancellationToken);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Load Person data for each player and create DTOs with team information
            List<FloorballPlayerDto> playerDtos = new List<FloorballPlayerDto>();
            foreach ((FloorballPlayer player, FloorballTeam? team) in pagedPlayersWithTeams.Items)
            {
                // Get the associated person
                Person? person = await _personRepository.GetByIdAsync(player.PersonId);
                
                // Create team DTO if team exists
                FloorballTeamNameDto? teamDto = team != null ? new FloorballTeamNameDto { Id = team.Id, Name = team.Name } : null;
                
                if (person != null)
                {
                    // Create DTO with real person data and team information
                    FloorballPlayerDto playerDto = new FloorballPlayerDto(
                        player.Id,
                        player.PersonId,
                        PersonMapper.ToDto(person),
                        player.IsActive,
                        player.Position.PrimaryPosition,
                        player.CareerGoals,
                        player.CareerAssists,
                        teamDto
                    );
                    playerDtos.Add(playerDto);
                }
                else
                {
                    // Fallback to placeholder if person not found
                    FloorballPlayerDto fallbackDto = FloorballPlayerMapper.ToDto(player);
                    // Create new DTO with team information
                    FloorballPlayerDto playerDtoWithTeam = new FloorballPlayerDto(
                        fallbackDto.Id,
                        fallbackDto.PersonId,
                        fallbackDto.Person,
                        fallbackDto.IsActive,
                        fallbackDto.Position,
                        fallbackDto.CareerGoals,
                        fallbackDto.CareerAssists,
                        teamDto
                    );
                    playerDtos.Add(playerDtoWithTeam);
                }
            }
            
            // Create the final paged result with DTOs
            PagedResult<FloorballPlayerDto> pagedResult = CreatePagedResult(
                playerDtos, 
                pagedPlayersWithTeams.TotalCount, 
                pagedPlayersWithTeams.Page, 
                pagedPlayersWithTeams.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball players out of {TotalCount} total", 
                pagedPlayersWithTeams.ItemCount, pagedPlayersWithTeams.TotalCount);

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
