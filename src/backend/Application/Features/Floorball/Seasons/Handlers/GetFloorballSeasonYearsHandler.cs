using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Seasons.Queries;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Seasons.Handlers;

/// <summary>
/// Builds distinct season-year options from league seasons.
/// </summary>
public class GetFloorballSeasonYearsHandler
    : IRequestHandler<GetFloorballSeasonYearsQuery, Result<IEnumerable<FloorballSeasonYearDto>>>
{
    private readonly IFloorballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFloorballSeasonYearsHandler> _logger;

    public GetFloorballSeasonYearsHandler(
        IFloorballCompetitionRepository competitionRepository,
        ILogger<GetFloorballSeasonYearsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FloorballSeasonYearDto>>> Handle(
        GetFloorballSeasonYearsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<FloorballSeasonDateSummary> summaries =
                await _competitionRepository.GetSeasonDateSummariesAsync(cancellationToken);

            List<FloorballSeasonYearDto> years = summaries
                .GroupBy(s => FloorballSeasonYear.FromDates(s.StartDate, s.EndDate))
                .Select(g => new FloorballSeasonYearDto(
                    g.Key,
                    g.Count(),
                    g.Any(x => x.IsActive)))
                .OrderByDescending(y =>
                {
                    FloorballSeasonYear.TryParse(y.Year, out int startYear, out int endYear);
                    return startYear;
                })
                .ThenByDescending(y =>
                {
                    FloorballSeasonYear.TryParse(y.Year, out int startYear, out int endYear);
                    return endYear;
                })
                .ToList();

            _logger.LogInformation("Retrieved {YearCount} floorball season years", years.Count);
            return Result<IEnumerable<FloorballSeasonYearDto>>.Success(years);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving floorball season years");
            return Result<IEnumerable<FloorballSeasonYearDto>>.Failure(
                "An error occurred while retrieving floorball season years.");
        }
    }
}
