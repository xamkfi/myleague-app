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
/// Handler for retrieving paginated football players with comprehensive filtering support
/// </summary>
public class GetAllFootballPlayersHandler : BasePagedQueryHandler<GetAllFootballPlayersQuery, FootballPlayerDto>,
    IRequestHandler<GetAllFootballPlayersQuery, Result<PagedResult<FootballPlayerDto>>>
{
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFootballPlayersHandler class
    /// </summary>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFootballPlayersHandler(
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IPaginationService paginationService,
        ILogger<GetAllFootballPlayersHandler> logger) : base(paginationService, logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
    }

    /// <summary>
    /// Handles the GetAllFootballPlayersQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of football players as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FootballPlayerDto>>> Handle(GetAllFootballPlayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving football players - Page: {Page}, PageSize: {PageSize}, IsActive: {IsActive}, Position: {Position}, TeamId: {TeamId}, SearchTerm: {SearchTerm}", 
                request.Page, request.PageSize, request.IsActive, request.Position, request.TeamId, request.SearchTerm);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllFootballPlayersQuery.ResourceKey);
            
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

            // Use repository-level pagination with all filters and team information
            PagedResult<(FootballPlayer Player, FootballTeam? Team)> pagedPlayersWithTeams = await _playerRepository.GetPagedWithTeamsAsync(
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
            List<FootballPlayerDto> playerDtos = new List<FootballPlayerDto>();
            foreach ((FootballPlayer player, FootballTeam? team) in pagedPlayersWithTeams.Items)
            {
                // Get the associated person
                Person? person = await _personRepository.GetByIdAsync(player.PersonId);
                
                // Create team DTO if team exists
                FootballTeamNameDto? teamDto = team != null ? new FootballTeamNameDto { Id = team.Id, Name = team.Name } : null;
                
                if (person != null)
                {
                    // Create DTO with real person data and team information
                    FootballPlayerDto playerDto = new FootballPlayerDto(
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
                    FootballPlayerDto fallbackDto = FootballPlayerMapper.ToDto(player);
                    // Create new DTO with team information
                    FootballPlayerDto playerDtoWithTeam = new FootballPlayerDto(
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
            PagedResult<FootballPlayerDto> pagedResult = CreatePagedResult(
                playerDtos, 
                pagedPlayersWithTeams.TotalCount, 
                pagedPlayersWithTeams.Page, 
                pagedPlayersWithTeams.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} football players out of {TotalCount} total", 
                pagedPlayersWithTeams.ItemCount, pagedPlayersWithTeams.TotalCount);

            return Result<PagedResult<FootballPlayerDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Football players retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football players");
            return Result<PagedResult<FootballPlayerDto>>.Failure("An error occurred while retrieving football players.");
        }
    }
} 
