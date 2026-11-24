using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Domain.Entities.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Handler for retrieving all player statistics for a specific team in a season
/// </summary>
public class GetTeamPlayerStatisticsHandler : IRequestHandler<GetFootballTeamPlayerStatisticsQuery, Result<List<FootballPlayerSeasonStatisticsDto>>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetTeamPlayerStatisticsHandler> _logger;

    public GetTeamPlayerStatisticsHandler(
        IFootballMatchRepository matchRepository,
        IFootballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetTeamPlayerStatisticsHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballPlayerSeasonStatisticsDto>>> Handle(GetFootballTeamPlayerStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving player statistics for Team {TeamId} in Season {SeasonId}", request.TeamId, request.CompetitionId);

            List<FootballPlayerSeasonStatistics> playerStats = await _statisticsRepository.GetPlayerStatisticsByTeamAndCompetitionAsync(
                request.TeamId, request.CompetitionId, cancellationToken);

            IEnumerable<Guid> personIds = playerStats.Select(x => x.Player.PersonId).ToList();
            IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);

            Dictionary<Guid, string> personLookup = persons.ToDictionary(p => p.Id, p => p.FullName);

            Dictionary<Guid, int> cleanSheets = CountCleanSheets(
                await _matchRepository.GetCompletedWithLineupsAsync(request.TeamId, request.CompetitionId, cancellationToken),
                request.TeamId);

            List<FootballPlayerSeasonStatisticsDto> dtos = playerStats.Select(stats =>
            {
                string playerName = personLookup.TryGetValue(stats.Player.PersonId, out string? fullName)
                    ? fullName
                    : string.Empty;

                FootballPlayerSeasonStatisticsDto dto = FootballStatisticsMapper.ToDto(stats, playerName);
                dto.CleanSheets = cleanSheets.GetValueOrDefault(stats.PlayerId);
                return dto;
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} player statistics for Team {TeamId} in Season {SeasonId}",
                dtos.Count, request.TeamId, request.CompetitionId);
            return Result<List<FootballPlayerSeasonStatisticsDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player statistics for Team {TeamId} in Season {SeasonId}",
                request.TeamId, request.CompetitionId);
            return Result<List<FootballPlayerSeasonStatisticsDto>>.Failure("An error occurred while retrieving team player statistics.");
        }
    }

    /// <summary>
    /// Counts clean sheets per goalkeeper. A goalkeeper stored on the bench did not keep goal.
    /// </summary>
    private static Dictionary<Guid, int> CountCleanSheets(IEnumerable<FootballMatch> matches, Guid teamId) =>
        matches
            .Where(match => match.HasCleanSheet(teamId))
            .SelectMany(match => match.Lineup
                .Where(p => p.TeamId == teamId && p.IsOnField && p.Position == FootballPosition.Goalkeeper)
                .Select(p => p.PlayerId)
                .Distinct())
            .GroupBy(playerId => playerId)
            .ToDictionary(group => group.Key, group => group.Count());
}
