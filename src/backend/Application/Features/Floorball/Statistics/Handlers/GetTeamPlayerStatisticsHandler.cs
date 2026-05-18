using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving all player statistics for a specific team in a season
/// </summary>
public class GetTeamPlayerStatisticsHandler : IRequestHandler<GetTeamPlayerStatisticsQuery, Result<List<FloorballPlayerSeasonStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetTeamPlayerStatisticsHandler> _logger;

    public GetTeamPlayerStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetTeamPlayerStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<List<FloorballPlayerSeasonStatisticsDto>>> Handle(GetTeamPlayerStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving player statistics for Team {TeamId} in Season {SeasonId}", request.TeamId, request.CompetitionId);

            List<FloorballPlayerSeasonStatistics> playerStats = await _statisticsRepository.GetPlayerStatisticsByTeamAndCompetitionAsync(
                request.TeamId, request.CompetitionId, cancellationToken);

            IEnumerable<Guid> personIds = playerStats.Select(x => x.Player.PersonId).ToList();
            IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);

            Dictionary<Guid, string> personLookup = persons.ToDictionary(p => p.Id, p => p.FullName);

            List<FloorballPlayerSeasonStatisticsDto> dtos = playerStats.Select(stats =>
            {
                string playerName = personLookup.TryGetValue(stats.Player.PersonId, out string? fullName)
                    ? fullName
                    : string.Empty;

                return FloorballStatisticsMapper.ToDto(stats, playerName);
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} player statistics for Team {TeamId} in Season {SeasonId}",
                dtos.Count, request.TeamId, request.CompetitionId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player statistics for Team {TeamId} in Season {SeasonId}",
                request.TeamId, request.CompetitionId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Failure("An error occurred while retrieving team player statistics.");
        }
    }
}
