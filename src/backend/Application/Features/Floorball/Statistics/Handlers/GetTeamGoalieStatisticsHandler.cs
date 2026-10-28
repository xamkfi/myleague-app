using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Statistics;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving all goalie statistics for a specific team in a competition
/// </summary>
public class GetTeamGoalieStatisticsHandler : IRequestHandler<GetFloorballTeamGoalieStatisticsQuery, Result<List<FloorballGoalieSeasonStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetTeamGoalieStatisticsHandler> _logger;

    public GetTeamGoalieStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetTeamGoalieStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<List<FloorballGoalieSeasonStatisticsDto>>> Handle(GetFloorballTeamGoalieStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving goalie statistics for Team {TeamId} in Competition {CompetitionId}", request.TeamId, request.CompetitionId);

        List<FloorballGoalieSeasonStatistics> goalieStats = await _statisticsRepository.GetGoalieStatisticsByTeamAndCompetitionAsync(
            request.TeamId, request.CompetitionId, cancellationToken);

        List<Guid> personIds = goalieStats.Select(x => x.Player.PersonId).Distinct().ToList();
        IEnumerable<Person> persons = personIds.Count == 0
            ? Enumerable.Empty<Person>()
            : await _personRepository.GetByIdsAsync(personIds);

        Dictionary<Guid, string> personLookup = persons.ToDictionary(p => p.Id, p => p.FullName);

        List<FloorballGoalieSeasonStatisticsDto> dtos = goalieStats.Select(stats =>
        {
            string playerName = personLookup.TryGetValue(stats.Player.PersonId, out string? fullName)
                ? fullName
                : string.Empty;

            return FloorballStatisticsMapper.ToDto(stats, playerName);
        }).ToList();

        return Result<List<FloorballGoalieSeasonStatisticsDto>>.Success(dtos);
    }
}
