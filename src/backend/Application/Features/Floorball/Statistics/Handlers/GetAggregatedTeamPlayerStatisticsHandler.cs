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
/// Returns per-player statistics for a team aggregated across every competition the team has
/// played in. Each player appears once with their totals summed from the regular season, every
/// tournament participation, etc. so the team page's player stats table shows the player's
/// true totals — not just the slice from the current league season.
/// </summary>
public class GetAggregatedTeamPlayerStatisticsHandler : IRequestHandler<GetAggregatedTeamPlayerStatisticsQuery, Result<List<FloorballPlayerSeasonStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetAggregatedTeamPlayerStatisticsHandler> _logger;

    public GetAggregatedTeamPlayerStatisticsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetAggregatedTeamPlayerStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<List<FloorballPlayerSeasonStatisticsDto>>> Handle(GetAggregatedTeamPlayerStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Aggregating per-player statistics across all competitions for Team {TeamId}", request.TeamId);

            List<FloorballPlayerSeasonStatistics> rows = await _statisticsRepository.GetPlayerStatisticsForTeamAsync(request.TeamId, cancellationToken);

            if (rows.Count == 0)
            {
                _logger.LogInformation("No player statistics found across competitions for Team {TeamId}", request.TeamId);
                return Result<List<FloorballPlayerSeasonStatisticsDto>>.Success(new List<FloorballPlayerSeasonStatisticsDto>());
            }

            // Resolve person names once so the per-player projection below can fill in PlayerName
            // without N+1 person lookups. Player.PersonId is loaded eagerly via the Include() chain
            // in the repository.
            IEnumerable<Guid> personIds = rows
                .Where(r => r.Player != null)
                .Select(r => r.Player.PersonId)
                .Distinct();
            IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);
            Dictionary<Guid, string> personLookup = persons.ToDictionary(p => p.Id, p => p.FullName);

            List<FloorballPlayerSeasonStatisticsDto> dtos = rows
                .GroupBy(r => r.PlayerId)
                .Select(group =>
                {
                    FloorballPlayerSeasonStatistics first = group.First();
                    string playerName = first.Player != null && personLookup.TryGetValue(first.Player.PersonId, out string? name)
                        ? name
                        : string.Empty;

                    return FloorballStatisticsMapper.AggregatePlayerStatistics(
                        playerId: group.Key,
                        teamId: request.TeamId,
                        playerName: playerName,
                        teamName: first.Team?.Name ?? string.Empty,
                        teamLogo: first.Team?.LogoUrl?.ToString(),
                        rows: group.ToList());
                })
                .OrderByDescending(dto => dto.Points)
                .ThenByDescending(dto => dto.Goals)
                .ThenByDescending(dto => dto.Assists)
                .ToList();

            _logger.LogInformation("Aggregated {RowCount} player stat rows into {PlayerCount} players for Team {TeamId}",
                rows.Count, dtos.Count, request.TeamId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating per-player team statistics for Team {TeamId}", request.TeamId);
            return Result<List<FloorballPlayerSeasonStatisticsDto>>.Failure("An error occurred while retrieving aggregated team player statistics.");
        }
    }
}
