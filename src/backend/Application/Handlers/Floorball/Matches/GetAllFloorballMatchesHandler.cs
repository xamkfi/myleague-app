using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Floorball.Match;
using System.Linq;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for retrieving paginated floorball matches with filtering support
/// </summary>
public class GetAllFloorballMatchesHandler : BasePagedQueryHandler<GetAllFloorballMatchesQuery, FloorballMatchDto>,
    IRequestHandler<GetAllFloorballMatchesQuery, Result<PagedResult<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballMatchesHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballMatchesHandler(
        IFloorballMatchRepository matchRepository,
        IPaginationService paginationService,
        ILogger<GetAllFloorballMatchesHandler> logger) : base(paginationService, logger)
    {
        _matchRepository = matchRepository;
    }

    /// <summary>
    /// Handles the GetAllFloorballMatchesQuery request
    /// </summary>
    /// <param name="request">The query containing pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of floorball matches as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<FloorballMatchDto>>> Handle(GetAllFloorballMatchesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving floorball matches - Page: {Page}, PageSize: {PageSize}, SeasonId: {SeasonId}, TeamId: {TeamId}, StartDate: {StartDate}, EndDate: {EndDate}", 
                request.Page, request.PageSize, request.SeasonId, request.TeamId, request.StartDate, request.EndDate);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllFloorballMatchesQuery.ResourceKey);
            
            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FloorballMatchDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get all matches and apply filtering (will be moved to repository level)
            IEnumerable<FloorballMatch> allMatches = await _matchRepository.GetAllAsync();
            
            // Apply filtering
            IEnumerable<FloorballMatch> filteredMatches = allMatches;
            
            if (request.SeasonId.HasValue)
            {
                filteredMatches = filteredMatches.Where(m => m.SeasonId == request.SeasonId.Value);
            }
            
            if (request.TeamId.HasValue)
            {
                filteredMatches = filteredMatches.Where(m => m.HomeTeamId == request.TeamId.Value || m.AwayTeamId == request.TeamId.Value);
            }
            
            if (request.StartDate.HasValue)
            {
                filteredMatches = filteredMatches.Where(m => m.ScheduledDateTime >= request.StartDate.Value);
            }
            
            if (request.EndDate.HasValue)
            {
                filteredMatches = filteredMatches.Where(m => m.ScheduledDateTime <= request.EndDate.Value);
            }

            // Apply pagination in memory (this will be moved to repository level)
            int totalCount = filteredMatches.Count();
            IEnumerable<FloorballMatch> matches = filteredMatches
                .OrderByDescending(m => m.ScheduledDateTime) // Default ordering by scheduled date
                .Skip((request.Page - 1) * actualPageSize)
                .Take(actualPageSize);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches);
            
            PagedResult<FloorballMatchDto> pagedResult = CreatePagedResult(
                matchDtos, 
                totalCount, 
                request.Page, 
                actualPageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball matches out of {TotalCount} total", 
                matches.Count(), totalCount);

            return Result<PagedResult<FloorballMatchDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Floorball matches retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball matches");
            return Result<PagedResult<FloorballMatchDto>>.Failure("An error occurred while retrieving floorball matches.");
        }
    }
} 
