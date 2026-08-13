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
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Football.Teams.Queries;
using Domain.Entities.Common;
using System.Linq;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for retrieving paginated football teams with filtering support
/// </summary>
public class GetAllFootballTeamsHandler : BasePagedQueryHandler<GetAllFootballTeamsQuery, FootballTeamDto>,
    IRequestHandler<GetAllFootballTeamsQuery, Result<PagedResult<FootballTeamDto>>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFootballTeamsHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFootballTeamsHandler(
        IFootballTeamRepository teamRepository,
        IPaginationService paginationService,
        IClubRepository clubRepository,
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        ILogger<GetAllFootballTeamsHandler> logger) : base(paginationService, logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
    }

    /// <summary>
    /// Handles the GetAllFootballTeamsQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of football teams as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FootballTeamDto>>> Handle(GetAllFootballTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving football teams - Page: {Page}, PageSize: {PageSize}, ClubId: {ClubId}, Division: {Division}", 
                request.Page, request.PageSize, request.ClubId, request.Division);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllFootballTeamsQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FootballTeamDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Parse division filter - treat Division as a Guid string
            Guid? divisionId = null;
            if (!string.IsNullOrEmpty(request.Division) && Guid.TryParse(request.Division, out Guid parsedDivisionId))
            {
                divisionId = parsedDivisionId;
            }

            // Get paginated teams using database-level pagination
            PagedResult<FootballTeam> pagedTeams = await _teamRepository.GetPagedAsync(
                page: request.Page,
                pageSize: actualPageSize,
                clubId: request.ClubId,
                divisionId: divisionId,
                teamCategories: request.TeamCategories,
                cancellationToken: cancellationToken);
            
            // Load all clubs for DTO mapping (since Club navigation is ignored in FootballTeam)
            IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
            Dictionary<Guid, Club> clubDictionary = new Dictionary<Guid, Club>();
            foreach (Club club in clubs)
            {
                clubDictionary[club.Id] = club;
            }

            // Load Person data for all players in all team rosters
            Dictionary<Guid, Person> playerPersons = new Dictionary<Guid, Person>();
            HashSet<Guid> allPlayerIds = new HashSet<Guid>();
            
            // Collect all unique player IDs from all teams
            foreach (FootballTeam team in pagedTeams.Items)
            {
                foreach (FootballTeamPlayer rosterPlayer in team.Roster)
                {
                    allPlayerIds.Add(rosterPlayer.PlayerId);
                }
            }
            
            // Load Person data for all unique players
            foreach (Guid playerId in allPlayerIds)
            {
                FootballPlayer? footballPlayer = await _playerRepository.GetByIdAsync(playerId);
                if (footballPlayer != null)
                {
                    Person? person = await _personRepository.GetByIdAsync(footballPlayer.PersonId);
                    if (person != null)
                    {
                        playerPersons[playerId] = person;
                    }
                }
            }

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<FootballTeamDto> teamDtos = FootballTeamMapper.ToDtos(pagedTeams.Items, clubDictionary, playerPersons);
            
            PagedResult<FootballTeamDto> pagedResult = CreatePagedResult(
                teamDtos, 
                pagedTeams.TotalCount, 
                pagedTeams.Page, 
                pagedTeams.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} football teams out of {TotalCount} total", 
                pagedTeams.ItemCount, pagedTeams.TotalCount);

            return Result<PagedResult<FootballTeamDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Football teams retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football teams");
            return Result<PagedResult<FootballTeamDto>>.Failure("An error occurred while retrieving football teams.");
        }
    }
} 
