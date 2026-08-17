using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Queries;
using Domain.Common;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class GetFootballSeasonsPagedHandler
    : IRequestHandler<GetFootballSeasonsPagedQuery, Result<PagedResult<FootballSeasonSummaryDto>>>
{
    private const int DefaultPageSize = 6;

    private readonly IFootballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFootballSeasonsPagedHandler> _logger;

    public GetFootballSeasonsPagedHandler(
        IFootballCompetitionRepository competitionRepository,
        ILogger<GetFootballSeasonsPagedHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<FootballSeasonSummaryDto>>> Handle(
        GetFootballSeasonsPagedQuery request,
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
                if (!FootballSeasonYear.TryParse(request.SeasonYear, out int parsedStart, out int parsedEnd))
                {
                    return Result<PagedResult<FootballSeasonSummaryDto>>.Failure(
                        "Invalid seasonYear. Use formats like '2024' or '2024-2025'.");
                }

                startYear = parsedStart;
                endYear = parsedEnd;
            }

            PagedResult<FootballSeason> paged = await _competitionRepository.GetSeasonsPagedAsync(
                page,
                pageSize,
                startYear,
                endYear,
                request.TeamCategory,
                cancellationToken);

            List<FootballSeasonSummaryDto> items = paged.Items
                .Select(s => new FootballSeasonSummaryDto(
                    s.Id,
                    s.Name,
                    s.StartDate,
                    s.EndDate,
                    s.IsActive,
                    s.IsCompleted,
                    FootballSeasonYear.FromDates(s.StartDate, s.EndDate),
                    s.TeamCategory))
                .ToList();

            PagedResult<FootballSeasonSummaryDto> result = PagedResult.Create(
                items,
                paged.TotalCount,
                paged.Page,
                paged.PageSize);

            _logger.LogInformation(
                "Retrieved football seasons page {Page}/{TotalPages} (year={SeasonYear}, count={Count})",
                result.Page,
                result.TotalPages,
                request.SeasonYear ?? "all",
                result.ItemCount);

            return Result<PagedResult<FootballSeasonSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged football seasons");
            return Result<PagedResult<FootballSeasonSummaryDto>>.Failure(
                "An error occurred while retrieving football seasons.");
        }
    }
}
