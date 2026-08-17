using Application.Common;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Mappings;
using Application.Features.Hockey.Statistics.Queries;
using Domain.Entities.Hockey.Statistics;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Gets one team's competition statistics.
/// </summary>
public class GetHockeyTeamCompetitionStatisticsHandler
    : IRequestHandler<GetHockeyTeamCompetitionStatisticsQuery, Result<HockeyTeamCompetitionStatisticsDto>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyTeamCompetitionStatisticsHandler> _logger;

    public GetHockeyTeamCompetitionStatisticsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyTeamCompetitionStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyTeamCompetitionStatisticsDto>> Handle(
        GetHockeyTeamCompetitionStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyStatisticsHandlerSupport.ValidateScopeIds(
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            HockeyTeamCompetitionStatistics? row =
                await _statisticsRepository.GetTeamCompetitionStatisticsAsync(
                    request.TeamId,
                    request.CompetitionId,
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            if (row is null)
                return Result<HockeyTeamCompetitionStatisticsDto>.NotFound(
                    "HockeyTeamCompetitionStatistics",
                    $"{request.TeamId}/{request.CompetitionId}");

            return Result<HockeyTeamCompetitionStatisticsDto>.Success(HockeyStatisticsMapper.ToDto(row));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyTeamCompetitionStatistics");
            return Result<HockeyTeamCompetitionStatisticsDto>.Failure(
                "An error occurred while retrieving team statistics.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets player competition statistics list or single.
/// </summary>
public class GetHockeyPlayerCompetitionStatisticsHandler
    : IRequestHandler<GetHockeyPlayerCompetitionStatisticsQuery, Result<List<HockeyPlayerCompetitionStatisticsDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyPlayerCompetitionStatisticsHandler> _logger;

    public GetHockeyPlayerCompetitionStatisticsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyPlayerCompetitionStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<HockeyPlayerCompetitionStatisticsDto>>> Handle(
        GetHockeyPlayerCompetitionStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyStatisticsHandlerSupport.ValidateScopeIds(
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            if (request.PlayerId is Guid playerId && request.TeamId is Guid teamId)
            {
                HockeyPlayerCompetitionStatistics? row =
                    await _statisticsRepository.GetPlayerCompetitionStatisticsAsync(
                        playerId,
                        teamId,
                        request.CompetitionId,
                        request.Scope,
                        request.CompetitionDivisionId,
                        request.TournamentGroupId,
                        request.PlayoffSeriesId);

                if (row is null)
                    return Result<List<HockeyPlayerCompetitionStatisticsDto>>.NotFound(
                        "HockeyPlayerCompetitionStatistics",
                        $"{playerId}/{teamId}/{request.CompetitionId}");

                return Result<List<HockeyPlayerCompetitionStatisticsDto>>.Success(
                    new List<HockeyPlayerCompetitionStatisticsDto> { HockeyStatisticsMapper.ToDto(row) });
            }

            IReadOnlyList<HockeyPlayerCompetitionStatistics> rows =
                await _statisticsRepository.GetPlayerCompetitionStatisticsAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            return Result<List<HockeyPlayerCompetitionStatisticsDto>>.Success(
                rows.Select(HockeyStatisticsMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyPlayerCompetitionStatistics");
            return Result<List<HockeyPlayerCompetitionStatisticsDto>>.Failure(
                "An error occurred while retrieving player statistics.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets goalie competition statistics list or single.
/// </summary>
public class GetHockeyGoalieCompetitionStatisticsHandler
    : IRequestHandler<GetHockeyGoalieCompetitionStatisticsQuery, Result<List<HockeyGoalieCompetitionStatisticsDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyGoalieCompetitionStatisticsHandler> _logger;

    public GetHockeyGoalieCompetitionStatisticsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyGoalieCompetitionStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<HockeyGoalieCompetitionStatisticsDto>>> Handle(
        GetHockeyGoalieCompetitionStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyStatisticsHandlerSupport.ValidateScopeIds(
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            if (request.PlayerId is Guid playerId && request.TeamId is Guid teamId)
            {
                HockeyGoalieCompetitionStatistics? row =
                    await _statisticsRepository.GetGoalieCompetitionStatisticsAsync(
                        playerId,
                        teamId,
                        request.CompetitionId,
                        request.Scope,
                        request.CompetitionDivisionId,
                        request.TournamentGroupId,
                        request.PlayoffSeriesId);

                if (row is null)
                    return Result<List<HockeyGoalieCompetitionStatisticsDto>>.NotFound(
                        "HockeyGoalieCompetitionStatistics",
                        $"{playerId}/{teamId}/{request.CompetitionId}");

                return Result<List<HockeyGoalieCompetitionStatisticsDto>>.Success(
                    new List<HockeyGoalieCompetitionStatisticsDto> { HockeyStatisticsMapper.ToDto(row) });
            }

            IReadOnlyList<HockeyGoalieCompetitionStatistics> rows =
                await _statisticsRepository.GetGoalieCompetitionStatisticsAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            return Result<List<HockeyGoalieCompetitionStatisticsDto>>.Success(
                rows.Select(HockeyStatisticsMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyGoalieCompetitionStatistics");
            return Result<List<HockeyGoalieCompetitionStatisticsDto>>.Failure(
                "An error occurred while retrieving goalie statistics.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets top scorers.
/// </summary>
public class GetHockeyTopScorersHandler
    : IRequestHandler<GetHockeyTopScorersQuery, Result<List<HockeyTopScorerDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyTopScorersHandler> _logger;

    public GetHockeyTopScorersHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyTopScorersHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<HockeyTopScorerDto>>> Handle(
        GetHockeyTopScorersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyStatisticsHandlerSupport.ValidateScopeIds(
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            IReadOnlyList<HockeyPlayerCompetitionStatistics> rows =
                await _statisticsRepository.GetTopScorersAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.TopN,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            return Result<List<HockeyTopScorerDto>>.Success(
                rows.Select(HockeyStatisticsMapper.ToTopScorerDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyTopScorers");
            return Result<List<HockeyTopScorerDto>>.Failure(
                "An error occurred while retrieving top scorers.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets top goalies.
/// </summary>
public class GetHockeyTopGoaliesHandler
    : IRequestHandler<GetHockeyTopGoaliesQuery, Result<List<HockeyTopGoalieDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyTopGoaliesHandler> _logger;

    public GetHockeyTopGoaliesHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyTopGoaliesHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<HockeyTopGoalieDto>>> Handle(
        GetHockeyTopGoaliesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyStatisticsHandlerSupport.ValidateScopeIds(
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            IReadOnlyList<HockeyGoalieCompetitionStatistics> rows =
                await _statisticsRepository.GetTopGoaliesAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.TopN,
                    request.MinimumGamesPlayed,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            return Result<List<HockeyTopGoalieDto>>.Success(
                rows.Select(HockeyStatisticsMapper.ToTopGoalieDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyTopGoalies");
            return Result<List<HockeyTopGoalieDto>>.Failure(
                "An error occurred while retrieving top goalies.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets competition statistics summary.
/// </summary>
public class GetHockeyCompetitionStatisticsSummaryHandler
    : IRequestHandler<GetHockeyCompetitionStatisticsSummaryQuery, Result<HockeyCompetitionStatisticsSummaryDto>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyCompetitionStatisticsSummaryHandler> _logger;

    public GetHockeyCompetitionStatisticsSummaryHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyCompetitionStatisticsSummaryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionStatisticsSummaryDto>> Handle(
        GetHockeyCompetitionStatisticsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyStatisticsHandlerSupport.ValidateScopeIds(
                request.Scope,
                request.CompetitionDivisionId,
                request.TournamentGroupId,
                request.PlayoffSeriesId);

            IReadOnlyList<HockeyTeamCompetitionStatistics> teams =
                await _statisticsRepository.GetTeamCompetitionStatisticsAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            IReadOnlyList<HockeyPlayerCompetitionStatistics> players =
                await _statisticsRepository.GetPlayerCompetitionStatisticsAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            IReadOnlyList<HockeyGoalieCompetitionStatistics> goalies =
                await _statisticsRepository.GetGoalieCompetitionStatisticsAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            IReadOnlyList<HockeyPlayerCompetitionStatistics> topScorers =
                await _statisticsRepository.GetTopScorersAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.TopN,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            IReadOnlyList<HockeyGoalieCompetitionStatistics> topGoalies =
                await _statisticsRepository.GetTopGoaliesAsync(
                    request.CompetitionId,
                    request.Scope,
                    request.TopN,
                    minimumGamesPlayed: 1,
                    request.CompetitionDivisionId,
                    request.TournamentGroupId,
                    request.PlayoffSeriesId);

            return Result<HockeyCompetitionStatisticsSummaryDto>.Success(new HockeyCompetitionStatisticsSummaryDto
            {
                CompetitionId = request.CompetitionId,
                Scope = request.Scope,
                CompetitionDivisionId = request.CompetitionDivisionId,
                TournamentGroupId = request.TournamentGroupId,
                PlayoffSeriesId = request.PlayoffSeriesId,
                TeamCount = teams.Count,
                PlayerCount = players.Count,
                GoalieCount = goalies.Count,
                Standings = teams.Select(HockeyStatisticsMapper.ToDto).ToList(),
                TopScorers = topScorers.Select(HockeyStatisticsMapper.ToTopScorerDto).ToList(),
                TopGoalies = topGoalies.Select(HockeyStatisticsMapper.ToTopGoalieDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyCompetitionStatisticsSummary");
            return Result<HockeyCompetitionStatisticsSummaryDto>.Failure(
                "An error occurred while retrieving competition statistics summary.",
                ex.Flatten());
        }
    }
}
