using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using Application.Features.Football.Seasons.Queries;
using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class GetFootballSeasonYearsHandler
    : IRequestHandler<GetFootballSeasonYearsQuery, Result<IEnumerable<FootballSeasonYearDto>>>
{
    private readonly IFootballCompetitionRepository _competitionRepository;
    private readonly ILogger<GetFootballSeasonYearsHandler> _logger;

    public GetFootballSeasonYearsHandler(
        IFootballCompetitionRepository competitionRepository,
        ILogger<GetFootballSeasonYearsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FootballSeasonYearDto>>> Handle(
        GetFootballSeasonYearsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<FootballSeasonDateSummary> summaries =
                await _competitionRepository.GetSeasonDateSummariesAsync(cancellationToken);

            List<FootballSeasonYearDto> years = summaries
                .GroupBy(s => FootballSeasonYear.FromDates(s.StartDate, s.EndDate))
                .Select(g => new FootballSeasonYearDto(
                    g.Key,
                    g.Count(),
                    g.Any(x => x.IsActive)))
                .OrderByDescending(y =>
                {
                    FootballSeasonYear.TryParse(y.Year, out int startYear, out int _);
                    return startYear;
                })
                .ThenByDescending(y =>
                {
                    FootballSeasonYear.TryParse(y.Year, out int _, out int endYear);
                    return endYear;
                })
                .ToList();

            _logger.LogInformation("Retrieved {YearCount} football season years", years.Count);
            return Result<IEnumerable<FootballSeasonYearDto>>.Success(years);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving football season years");
            return Result<IEnumerable<FootballSeasonYearDto>>.Failure(
                "An error occurred while retrieving football season years.");
        }
    }
}
