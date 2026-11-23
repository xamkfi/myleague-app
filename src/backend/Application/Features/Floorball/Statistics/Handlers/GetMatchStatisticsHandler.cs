using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Entities.Floorball.Matches;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving match statistics
/// </summary>
public class GetMatchStatisticsHandler : IRequestHandler<GetFloorballMatchStatisticsQuery, Result<List<FloorballMatchTeamStatisticsDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetMatchStatisticsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetMatchStatisticsHandler class
    /// </summary>
    /// <param name="matchRepository">The match repository</param>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetMatchStatisticsHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballStatisticsRepository statisticsRepository,
        ILogger<GetMatchStatisticsHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballMatchStatisticsQuery request
    /// </summary>
    /// <param name="request">The query containing the match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of match team statistics DTOs</returns>
    public async Task<Result<List<FloorballMatchTeamStatisticsDto>>> Handle(GetFloorballMatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting match statistics for Match: {MatchId}", request.MatchId);

        List<Domain.Entities.Floorball.Statistics.FloorballMatchTeamStatistics> matchStats = 
            (await _statisticsRepository.GetMatchStatisticsAsync(request.MatchId, cancellationToken)).ToList();

        List<FloorballMatchTeamStatisticsDto> dtos = matchStats
            .Select(ms => FloorballStatisticsMapper.ToDto(ms))
            .ToList();

        // The events are the truth for shots. Counting them here also corrects matches whose stored
        // totals drifted when saves were deleted and recorded again.
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match != null)
        {
            foreach (FloorballMatchTeamStatisticsDto dto in dtos)
            {
                FloorballTeamShots shots = match.CalculateTeamShots(dto.TeamId);
                dto.ShotsOnGoal = shots.OnGoal;
                dto.ShotsTotal = shots.Total;
                dto.ShotPercentage = shots.Total > 0 ? (decimal)shots.OnGoal / shots.Total * 100 : 0;
            }
        }

        return Result<List<FloorballMatchTeamStatisticsDto>>.Success(dtos);
    }
}
