using Application.Queries.Floorball.Team;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Mappings.Common;
using Application.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for retrieving paginated floorball teams without roster with filtering support
/// </summary>
public class GetAllTeamsWithoutRosterHandler : BasePagedQueryHandler<GetAllTeamsWithoutRosterQuery, FloorballTeamSummaryDto>,
    IRequestHandler<GetAllTeamsWithoutRosterQuery, Result<PagedResult<FloorballTeamSummaryDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllTeamsWithoutRosterHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetAllTeamsWithoutRosterHandler(
        IFloorballTeamRepository teamRepository,
        IPaginationService paginationService,
        IClubRepository clubRepository,
        ILogger<GetAllTeamsWithoutRosterHandler> logger) : base(paginationService, logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
    }

    /// <summary>
    /// Handles the GetAllTeamsWithoutRosterQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of floorball teams as summary DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FloorballTeamSummaryDto>>> Handle(GetAllTeamsWithoutRosterQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving floorball teams without roster - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, TeamCategory: {TeamCategory}", 
                request.Page, request.PageSize, request.SearchTerm, request.TeamCategory);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllTeamsWithoutRosterQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FloorballTeamSummaryDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get paginated teams using database-level pagination (without roster)
            PagedResult<FloorballTeam> pagedTeams = await _teamRepository.GetAllTeamsWithoutRosterAsync(
                page: request.Page,
                pageSize: actualPageSize,
                searchTerm: request.SearchTerm,
                teamCategory: request.TeamCategory,
                cancellationToken: cancellationToken);
            
            // Load all clubs for DTO mapping (since Club navigation is ignored in FloorballTeam)
            IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
            Dictionary<Guid, Club> clubDictionary = new Dictionary<Guid, Club>();
            foreach (Club club in clubs)
            {
                clubDictionary[club.Id] = club;
            }

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Map to summary DTOs (without roster)
            // TeamCategory filtering is done at the repository level
            IEnumerable<FloorballTeamSummaryDto> teamDtos = FloorballTeamMapper.ToSummaryDtos(pagedTeams.Items, clubDictionary);
            
            PagedResult<FloorballTeamSummaryDto> pagedResult = CreatePagedResult(
                teamDtos, 
                pagedTeams.TotalCount, 
                pagedTeams.Page, 
                pagedTeams.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball teams without roster out of {TotalCount} total", 
                teamDtos.Count(), pagedTeams.TotalCount);

            return Result<PagedResult<FloorballTeamSummaryDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Floorball teams retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball teams without roster");
            return Result<PagedResult<FloorballTeamSummaryDto>>.Failure("An error occurred while retrieving floorball teams without roster.");
        }
    }
}

