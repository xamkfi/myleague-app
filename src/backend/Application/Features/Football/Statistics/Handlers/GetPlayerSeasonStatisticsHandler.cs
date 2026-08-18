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
/// Handler for retrieving player season statistics
/// </summary>
public class GetPlayerSeasonStatisticsHandler : IRequestHandler<GetPlayerSeasonStatisticsQuery, Result<FootballPlayerSeasonStatisticsDto>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetPlayerSeasonStatisticsHandler> _logger;

    public GetPlayerSeasonStatisticsHandler(
        IFootballStatisticsRepository statisticsRepository,
        IPersonRepository personRepository,
        ILogger<GetPlayerSeasonStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<FootballPlayerSeasonStatisticsDto>> Handle(GetPlayerSeasonStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);

            IEnumerable<FootballPlayerSeasonStatistics> allPlayerStats =
                await _statisticsRepository.GetPlayerStatisticsByCompetitionAsync(request.CompetitionId, cancellationToken);

            FootballPlayerSeasonStatistics? playerStats =
                allPlayerStats.FirstOrDefault(ps => ps.PlayerId == request.PlayerId);

            if (playerStats == null)
            {
                _logger.LogWarning("Player season statistics not found for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
                return Result<FootballPlayerSeasonStatisticsDto>.NotFound("Player season statistics", $"Player {request.PlayerId} in season {request.CompetitionId}");
            }

            Person? person = await _personRepository.GetByIdAsync(playerStats.Player.PersonId);

            if (person == null)
            {
                _logger.LogWarning("Player season statistics not found for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
                return Result<FootballPlayerSeasonStatisticsDto>.NotFound("Player season statistics", $"Player {request.PlayerId} in season {request.CompetitionId}");
            }

            FootballPlayerSeasonStatisticsDto dto = FootballStatisticsMapper.ToDto(playerStats, person.FullName);

            _logger.LogInformation("Successfully retrieved player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
            return Result<FootballPlayerSeasonStatisticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting player season statistics for Player: {PlayerId} in Season: {SeasonId}", request.PlayerId, request.CompetitionId);
            return Result<FootballPlayerSeasonStatisticsDto>.Failure("An error occurred while retrieving player season statistics.");
        }
    }
}
