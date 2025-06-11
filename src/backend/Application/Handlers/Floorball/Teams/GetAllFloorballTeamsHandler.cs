using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Enums.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Floorball.Team;
using System.Linq;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for retrieving paginated floorball teams with filtering support
/// </summary>
public class GetAllFloorballTeamsHandler : BasePagedQueryHandler<GetAllFloorballTeamsQuery, FloorballTeamDto>,
    IRequestHandler<GetAllFloorballTeamsQuery, Result<PagedResult<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballTeamsHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballTeamsHandler(
        IFloorballTeamRepository teamRepository,
        IPaginationService paginationService,
        ILogger<GetAllFloorballTeamsHandler> logger) : base(paginationService, logger)
    {
        _teamRepository = teamRepository;
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

            // Get all teams and apply filtering (will be moved to repository level)
            IEnumerable<FloorballTeam> allTeams = await _teamRepository.GetAllAsync();
            
            // Apply filtering
            IEnumerable<FloorballTeam> filteredTeams = allTeams;
            
            if (request.ClubId.HasValue)
            {
                filteredTeams = filteredTeams.Where(t => t.Club.Id == request.ClubId.Value);
            }
            
            if (!string.IsNullOrEmpty(request.Division))
            {
                if (Enum.TryParse<FloorballDivision>(request.Division, true, out FloorballDivision division))
                {
                    filteredTeams = filteredTeams.Where(t => t.Division == division);
                }
            }

            // Apply pagination in memory (this will be moved to repository level)
            int totalCount = filteredTeams.Count();
            IEnumerable<FloorballTeam> teams = filteredTeams
                .Skip((request.Page - 1) * actualPageSize)
                .Take(actualPageSize);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams);
            
            PagedResult<FloorballTeamDto> pagedResult = CreatePagedResult(
                teamDtos, 
                totalCount, 
                request.Page, 
                actualPageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball teams out of {TotalCount} total", 
                teams.Count(), totalCount);

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
