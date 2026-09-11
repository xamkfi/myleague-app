using Application.Features.Common.Clubs.Queries;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Application.Services.Common;
using Domain.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Linq;

namespace Application.Features.Common.Clubs.Handlers;

/// <summary>
/// Handler for retrieving clubs with pagination support
/// </summary>
public class GetAllClubsHandler : BasePagedQueryHandler<GetAllClubsQuery, ClubDto>, IRequestHandler<GetAllClubsQuery, Result<PagedResult<ClubDto>>>
{
    private readonly IClubRepository _clubRepository;

    /// <summary>
    /// Initializes a new instance of the GetAllClubsHandler class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    /// <param name="paginationService">The pagination service</param>
    public GetAllClubsHandler(
        IClubRepository clubRepository, 
        ILogger<GetAllClubsHandler> logger,
        IPaginationService paginationService) : base(paginationService, logger)
    {
        _clubRepository = clubRepository;
    }

    /// <summary>
    /// Handles the GetAllClubsQuery request
    /// </summary>
    /// <param name="request">The query containing pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated collection of clubs as DTOs wrapped in a Result</returns>
    public async Task<Result<PagedResult<ClubDto>>> Handle(GetAllClubsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving clubs - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            // Validate pagination parameters using base handler
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page, request.PageSize, GetAllClubsQuery.ResourceKey);

            if (validationResult.IsFailure)
            {
                return Result<PagedResult<ClubDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            // Check for cancellation before database operations
            cancellationToken.ThrowIfCancellationRequested();

            // Get paginated clubs using database-level pagination
            PagedResult<Domain.Entities.Common.Club> pagedClubs = await _clubRepository.GetPagedAsync(
                request.Page,
                actualPageSize,
                cancellationToken);

            IEnumerable<ClubDto> clubDtos = ClubMapper.ToDtos(pagedClubs.Items);

            // Check for cancellation after database operations
            cancellationToken.ThrowIfCancellationRequested();

            PagedResult<ClubDto> pagedResult = CreatePagedResult(
                clubDtos, 
                pagedClubs.TotalCount, 
                pagedClubs.Page, 
                pagedClubs.PageSize);

            _logger.LogInformation("Successfully retrieved {Count} clubs out of {TotalCount} total", 
                pagedClubs.ItemCount, pagedClubs.TotalCount);

            return Result<PagedResult<ClubDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Clubs retrieval was cancelled - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            throw; // Re-throw to let the framework handle it
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clubs");
            return Result<PagedResult<ClubDto>>.Failure("An error occurred while retrieving clubs.");
        }
    }
}


