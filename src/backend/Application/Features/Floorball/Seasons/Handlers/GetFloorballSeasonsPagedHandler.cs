using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Queries;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Returns a slim, paginated list of floorball seasons for the public sports page.
/// </summary>
public class GetFloorballSeasonsPagedHandler
    : IRequestHandler<GetFloorballSeasonsPagedQuery, Result<PagedResult<FloorballSeasonSummaryDto>>>
{
    private const int DefaultPageSize = 6;

    private readonly IFloorballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFloorballSeasonsPagedHandler> _logger;

    public GetFloorballSeasonsPagedHandler(
        IFloorballCompetitionRepository competitionRepository,
        ILogger<GetFloorballSeasonsPagedHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<FloorballSeasonSummaryDto>>> Handle(
        GetFloorballSeasonsPagedQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize <= 0 ? DefaultPageSize : Math.Min(request.PageSize, 100);

            int? startYear = null;
            int? endYear = null;
            if (!string.IsNullOrWhiteSpace(request.SeasonYear))
            {
                if (!FloorballSeasonYear.TryParse(request.SeasonYear, out int parsedStart, out int parsedEnd))
                {
                    return Result<PagedResult<FloorballSeasonSummaryDto>>.Failure(
                        "Invalid seasonYear. Use formats like '2024' or '2024-2025'.");
                }

                startYear = parsedStart;
                endYear = parsedEnd;
            }

            PagedResult<FloorballSeason> paged = await _competitionRepository.GetSeasonsPagedAsync(
                page,
                pageSize,
                startYear,
                endYear,
                cancellationToken);

            List<FloorballSeasonSummaryDto> items = paged.Items
                .Select(s => new FloorballSeasonSummaryDto(
                    s.Id,
                    s.Name,
                    s.StartDate,
                    s.EndDate,
                    s.IsActive,
                    s.IsCompleted,
                    FloorballSeasonYear.FromDates(s.StartDate, s.EndDate)))
                .ToList();

            PagedResult<FloorballSeasonSummaryDto> result = PagedResult.Create(
                items,
                paged.TotalCount,
                paged.Page,
                paged.PageSize);

            _logger.LogInformation(
                "Retrieved floorball seasons page {Page}/{TotalPages} (year={SeasonYear}, count={Count})",
                result.Page,
                result.TotalPages,
                request.SeasonYear ?? "all",
                result.ItemCount);

            return Result<PagedResult<FloorballSeasonSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged floorball seasons");
            return Result<PagedResult<FloorballSeasonSummaryDto>>.Failure(
                "An error occurred while retrieving floorball seasons.");
        }
    }
}
