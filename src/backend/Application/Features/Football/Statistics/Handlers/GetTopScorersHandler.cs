using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Entities.Football.Statistics;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Handler for retrieving top scorers for a season
/// </summary>
public class GetTopScorersHandler : IRequestHandler<GetTopScorersQuery, Result<List<FootballPlayerSeasonStatisticsDto>>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetTopScorersHandler> _logger;

    public GetTopScorersHandler(
        IFootballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetTopScorersHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballPlayerSeasonStatisticsDto>>> Handle(GetTopScorersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving top {TopN} scorers for Season {SeasonId}", request.TopN, request.CompetitionId);

            List<FootballPlayerSeasonStatistics> topScorers = await _statisticsRepository.GetTopScorersAsync(request.CompetitionId, request.TopN, cancellationToken);

            IEnumerable<Guid> personIds = topScorers.Select(x => x.Player.PersonId).ToList();
            IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);

            Dictionary<Guid, string> personLookup = persons.ToDictionary(p => p.Id, p => p.FullName);

            List<FootballPlayerSeasonStatisticsDto> dtos = topScorers.Select(stats =>
            {
                string playerName = personLookup.TryGetValue(stats.Player.PersonId, out string? fullName)
                    ? fullName
                    : string.Empty;

                return FootballStatisticsMapper.ToDto(stats, playerName);
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} top scorers for Season {SeasonId}", dtos.Count, request.CompetitionId);
            return Result<List<FootballPlayerSeasonStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving top scorers for Season {SeasonId}", request.CompetitionId);
            return Result<List<FootballPlayerSeasonStatisticsDto>>.Failure("An error occurred while retrieving top scorers.");
        }
    }
}
