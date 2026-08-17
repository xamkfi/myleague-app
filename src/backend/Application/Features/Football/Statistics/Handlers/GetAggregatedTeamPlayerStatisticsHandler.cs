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
/// Returns per-player statistics for a team aggregated across every competition the team has
/// played in.
/// </summary>
public class GetAggregatedTeamPlayerStatisticsHandler : IRequestHandler<GetAggregatedTeamPlayerStatisticsQuery, Result<List<FootballPlayerSeasonStatisticsDto>>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetAggregatedTeamPlayerStatisticsHandler> _logger;

    public GetAggregatedTeamPlayerStatisticsHandler(
        IFootballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetAggregatedTeamPlayerStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballPlayerSeasonStatisticsDto>>> Handle(GetAggregatedTeamPlayerStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Aggregating per-player statistics across all competitions for Team {TeamId}", request.TeamId);

            List<FootballPlayerSeasonStatistics> rows = await _statisticsRepository.GetPlayerStatisticsForTeamAsync(request.TeamId, cancellationToken);

            if (rows.Count == 0)
            {
                _logger.LogInformation("No player statistics found across competitions for Team {TeamId}", request.TeamId);
                return Result<List<FootballPlayerSeasonStatisticsDto>>.Success(new List<FootballPlayerSeasonStatisticsDto>());
            }

            IEnumerable<Guid> personIds = rows
                .Where(r => r.Player != null)
                .Select(r => r.Player.PersonId)
                .Distinct();
            IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);
            Dictionary<Guid, string> personLookup = persons.ToDictionary(p => p.Id, p => p.FullName);

            List<FootballPlayerSeasonStatisticsDto> dtos = rows
                .GroupBy(r => r.PlayerId)
                .Select(group =>
                {
                    FootballPlayerSeasonStatistics first = group.First();
                    string playerName = first.Player != null && personLookup.TryGetValue(first.Player.PersonId, out string? name)
                        ? name
                        : string.Empty;

                    return FootballStatisticsMapper.AggregatePlayerStatistics(
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
            return Result<List<FootballPlayerSeasonStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating per-player team statistics for Team {TeamId}", request.TeamId);
            return Result<List<FootballPlayerSeasonStatisticsDto>>.Failure("An error occurred while retrieving aggregated team player statistics.");
        }
    }
}
