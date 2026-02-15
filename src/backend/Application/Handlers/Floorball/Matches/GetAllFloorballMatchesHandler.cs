using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Common;
using Application.Handlers.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Entities.Common;
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
    private readonly IClubRepository _clubRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballMatchesHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballMatchesHandler(
        IFloorballMatchRepository matchRepository,
        IClubRepository clubRepository,
        IPaginationService paginationService,
        ILogger<GetAllFloorballMatchesHandler> logger) : base(paginationService, logger)
    {
        _matchRepository = matchRepository;
        _clubRepository = clubRepository;
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

            _logger.LogInformation("Retrieving floorball matches - Page: {Page}, PageSize: {PageSize}, SeasonId: {SeasonId}, TeamId: {TeamId}, StartDate: {StartDate}, EndDate: {EndDate}, SearchQuery: {SearchQuery}, Status: {Status}", 
                request.Page, request.PageSize, request.SeasonId, request.TeamId, request.StartDate, request.EndDate, request.SearchQuery, request.Status);

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

            // Use repository-level pagination with all filters
            PagedResult<FloorballMatch> pagedMatches = await _matchRepository.GetPagedAsync(
                page: validationResult.Data.Page,
                pageSize: actualPageSize,
                seasonId: request.SeasonId,
                teamId: request.TeamId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                status: request.Status,
                sortOrder: request.SortOrder,
                searchQuery: request.SearchQuery,
                cancellationToken: cancellationToken);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Load clubs for logo resolution (cross-context)
            List<Guid> clubIds = pagedMatches.Items
                .SelectMany(m => new[] { m.HomeTeam.ClubId, m.AwayTeam.ClubId })
                .Distinct()
                .ToList();

            Dictionary<Guid, Club> clubLookup = await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            // Map to DTOs with club data
            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(pagedMatches.Items, clubLookup);
            
            // Create the final paged result with DTOs
            PagedResult<FloorballMatchDto> pagedResult = CreatePagedResult(
                matchDtos, 
                pagedMatches.TotalCount, 
                pagedMatches.Page, 
                pagedMatches.PageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} floorball matches out of {TotalCount} total", 
                pagedMatches.ItemCount, pagedMatches.TotalCount);

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
