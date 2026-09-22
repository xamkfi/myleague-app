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
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Repositories.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Floorball.Teams.Queries;
using Domain.Entities.Common;
using System.Linq;

namespace Application.Features.Floorball.Teams.Handlers;

/// <summary>
/// Handler for retrieving paginated floorball teams with filtering support
/// </summary>
public class GetAllFloorballTeamsHandler : BasePagedQueryHandler<GetAllFloorballTeamsQuery, FloorballTeamDto>,
    IRequestHandler<GetAllFloorballTeamsQuery, Result<PagedResult<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballTeamsHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballTeamsHandler(
        IFloorballTeamRepository teamRepository,
        IPaginationService paginationService,
        IClubRepository clubRepository,
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        ILogger<GetAllFloorballTeamsHandler> logger) : base(paginationService, logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
    }

    /// <summary>
    /// Handles the GetAllFloorballTeamsQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of floorball teams as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FloorballTeamDto>>> Handle(GetAllFloorballTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving floorball teams - Page: {Page}, PageSize: {PageSize}, ClubId: {ClubId}, Division: {Division}", 
                request.Page, request.PageSize, request.ClubId, request.Division);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllFloorballTeamsQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FloorballTeamDto>>.Failure(validationResult.Error!);
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
            PagedResult<FloorballTeam> pagedTeams = await _teamRepository.GetPagedAsync(
                page: request.Page,
                pageSize: actualPageSize,
                searchTerm: request.SearchTerm ?? string.Empty,
                clubId: request.ClubId,
                divisionId: divisionId,
                teamCategories: request.TeamCategories,
                cancellationToken: cancellationToken);
            
            HashSet<Guid> clubIds = pagedTeams.Items.Select(team => team.ClubId).ToHashSet();
            Dictionary<Guid, Club> clubDictionary = await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            HashSet<Guid> allPlayerIds = new HashSet<Guid>();
            foreach (FloorballTeam team in pagedTeams.Items)
            {
                foreach (FloorballTeamPlayer rosterPlayer in team.Roster)
                {
                    allPlayerIds.Add(rosterPlayer.PlayerId);
                }
            }

            Dictionary<Guid, Person> playerPersons = new Dictionary<Guid, Person>();
            if (allPlayerIds.Count > 0)
            {
                Dictionary<Guid, FloorballPlayer> players = await _playerRepository.GetByIdsAsync(allPlayerIds, cancellationToken);
                IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(
                    players.Values.Select(player => player.PersonId).Distinct());
                Dictionary<Guid, Person> personsById = persons.ToDictionary(person => person.Id);
                foreach (KeyValuePair<Guid, FloorballPlayer> entry in players)
                {
                    if (personsById.TryGetValue(entry.Value.PersonId, out Person? person))
                    {
                        playerPersons[entry.Key] = person;
                    }
                }
            }

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(pagedTeams.Items, clubDictionary, playerPersons);
            
            PagedResult<FloorballTeamDto> pagedResult = CreatePagedResult(
                teamDtos, 
                pagedTeams.TotalCount, 
                pagedTeams.Page, 
                pagedTeams.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball teams out of {TotalCount} total", 
                pagedTeams.ItemCount, pagedTeams.TotalCount);

            return Result<PagedResult<FloorballTeamDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Floorball teams retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball teams");
            return Result<PagedResult<FloorballTeamDto>>.Failure("An error occurred while retrieving floorball teams.");
        }
    }
} 
