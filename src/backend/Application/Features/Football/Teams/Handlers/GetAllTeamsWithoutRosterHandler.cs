using Application.Features.Football.Teams.Queries;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
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

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for retrieving paginated football teams without roster with filtering support
/// </summary>
public class GetAllTeamsWithoutRosterHandler : BasePagedQueryHandler<GetAllTeamsWithoutRosterQuery, FootballTeamSummaryDto>,
    IRequestHandler<GetAllTeamsWithoutRosterQuery, Result<PagedResult<FootballTeamSummaryDto>>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllTeamsWithoutRosterHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetAllTeamsWithoutRosterHandler(
        IFootballTeamRepository teamRepository,
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
    /// <returns>A paginated collection of football teams as summary DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FootballTeamSummaryDto>>> Handle(GetAllTeamsWithoutRosterQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving football teams without roster - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, TeamCategory: {TeamCategory}", 
                request.Page, request.PageSize, request.SearchTerm, request.TeamCategory);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllTeamsWithoutRosterQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FootballTeamSummaryDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get paginated teams using database-level pagination (without roster)
            PagedResult<FootballTeam> pagedTeams = await _teamRepository.GetAllTeamsWithoutRosterAsync(
                page: request.Page,
                pageSize: actualPageSize,
                searchTerm: request.SearchTerm,
                teamCategory: request.TeamCategory,
                cancellationToken: cancellationToken);
            
            // Load all clubs for DTO mapping (since Club navigation is ignored in FootballTeam)
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
            IEnumerable<FootballTeamSummaryDto> teamDtos = FootballTeamMapper.ToSummaryDtos(pagedTeams.Items, clubDictionary);
            
            PagedResult<FootballTeamSummaryDto> pagedResult = CreatePagedResult(
                teamDtos, 
                pagedTeams.TotalCount, 
                pagedTeams.Page, 
                pagedTeams.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} football teams without roster out of {TotalCount} total", 
                teamDtos.Count(), pagedTeams.TotalCount);

            return Result<PagedResult<FootballTeamSummaryDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Football teams retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football teams without roster");
            return Result<PagedResult<FootballTeamSummaryDto>>.Failure("An error occurred while retrieving football teams without roster.");
        }
    }
}

