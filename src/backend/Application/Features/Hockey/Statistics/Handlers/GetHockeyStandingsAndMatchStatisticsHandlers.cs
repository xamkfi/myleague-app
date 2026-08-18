using Application.Common;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Mappings;
using Application.Features.Hockey.Statistics.Queries;
using Domain.Entities.Hockey.Statistics;
using Domain.Enums.Hockey.Statistics;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Statistics.Handlers;

/// <summary>
/// Gets match box score statistics.
/// </summary>
public class GetHockeyMatchStatisticsHandler
    : IRequestHandler<GetHockeyMatchStatisticsQuery, Result<HockeyMatchStatisticsDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyMatchStatisticsHandler> _logger;

    public GetHockeyMatchStatisticsHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyMatchStatisticsHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchStatisticsDto>> Handle(
        GetHockeyMatchStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await _matchRepository.GetByIdAsync(request.MatchId) is null)
                return Result<HockeyMatchStatisticsDto>.NotFound("HockeyMatch", request.MatchId);

            IReadOnlyList<HockeyMatchTeamStatistics> teams =
                await _statisticsRepository.GetMatchTeamStatisticsAsync(request.MatchId);
            IReadOnlyList<HockeyMatchPlayerStatistics> players =
                await _statisticsRepository.GetMatchPlayerStatisticsAsync(request.MatchId);
            IReadOnlyList<HockeyGoalieMatchStatistics> goalies =
                await _statisticsRepository.GetGoalieMatchStatisticsAsync(request.MatchId);

            return Result<HockeyMatchStatisticsDto>.Success(
                HockeyStatisticsMapper.ToMatchStatisticsDto(request.MatchId, teams, players, goalies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyMatchStatistics for {MatchId}", request.MatchId);
            return Result<HockeyMatchStatisticsDto>.Failure(
                "An error occurred while retrieving match statistics.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets competition standings.
/// </summary>
public class GetHockeyCompetitionStandingsHandler
    : IRequestHandler<GetHockeyCompetitionStandingsQuery, Result<List<HockeyTeamCompetitionStatisticsDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyCompetitionStandingsHandler> _logger;

    public GetHockeyCompetitionStandingsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyCompetitionStandingsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<HockeyTeamCompetitionStatisticsDto>>> Handle(
        GetHockeyCompetitionStandingsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyTeamCompetitionStatistics> rows =
                await _statisticsRepository.GetTeamCompetitionStatisticsAsync(
                    request.CompetitionId,
                    HockeyStatisticsScope.Competition);

            return Result<List<HockeyTeamCompetitionStatisticsDto>>.Success(
                rows.Select(HockeyStatisticsMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyCompetitionStandings for {CompetitionId}", request.CompetitionId);
            return Result<List<HockeyTeamCompetitionStatisticsDto>>.Failure(
                "An error occurred while retrieving standings.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets division standings.
/// </summary>
public class GetHockeyDivisionStandingsHandler
    : IRequestHandler<GetHockeyDivisionStandingsQuery, Result<List<HockeyTeamCompetitionStatisticsDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyDivisionStandingsHandler> _logger;

    public GetHockeyDivisionStandingsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyDivisionStandingsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<HockeyTeamCompetitionStatisticsDto>>> Handle(
        GetHockeyDivisionStandingsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyTeamCompetitionStatistics> rows =
                await _statisticsRepository.GetTeamCompetitionStatisticsAsync(
                    request.CompetitionId,
                    HockeyStatisticsScope.Division,
                    competitionDivisionId: request.CompetitionDivisionId);

            return Result<List<HockeyTeamCompetitionStatisticsDto>>.Success(
                rows.Select(HockeyStatisticsMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyDivisionStandings for {CompetitionId}", request.CompetitionId);
            return Result<List<HockeyTeamCompetitionStatisticsDto>>.Failure(
                "An error occurred while retrieving division standings.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets tournament group standings.
/// </summary>
public class GetHockeyTournamentGroupStandingsHandler
    : IRequestHandler<GetHockeyTournamentGroupStandingsQuery, Result<List<HockeyTeamCompetitionStatisticsDto>>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyTournamentGroupStandingsHandler> _logger;

    public GetHockeyTournamentGroupStandingsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyTournamentGroupStandingsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<List<HockeyTeamCompetitionStatisticsDto>>> Handle(
        GetHockeyTournamentGroupStandingsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyTeamCompetitionStatistics> rows =
                await _statisticsRepository.GetTeamCompetitionStatisticsAsync(
                    request.CompetitionId,
                    HockeyStatisticsScope.TournamentGroup,
                    tournamentGroupId: request.TournamentGroupId);

            return Result<List<HockeyTeamCompetitionStatisticsDto>>.Success(
                rows.Select(HockeyStatisticsMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyTournamentGroupStandings for {CompetitionId}", request.CompetitionId);
            return Result<List<HockeyTeamCompetitionStatisticsDto>>.Failure(
                "An error occurred while retrieving tournament group standings.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Gets playoff series statistics.
/// </summary>
public class GetHockeyPlayoffSeriesStatisticsHandler
    : IRequestHandler<GetHockeyPlayoffSeriesStatisticsQuery, Result<HockeyPlayoffSeriesStatisticsDto>>
{
    private readonly IHockeyStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetHockeyPlayoffSeriesStatisticsHandler> _logger;

    public GetHockeyPlayoffSeriesStatisticsHandler(
        IHockeyStatisticsRepository statisticsRepository,
        ILogger<GetHockeyPlayoffSeriesStatisticsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyPlayoffSeriesStatisticsDto>> Handle(
        GetHockeyPlayoffSeriesStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyTeamCompetitionStatistics> teams =
                await _statisticsRepository.GetTeamCompetitionStatisticsAsync(
                    request.CompetitionId,
                    HockeyStatisticsScope.PlayoffSeries,
                    playoffSeriesId: request.PlayoffSeriesId);

            IReadOnlyList<HockeyPlayerCompetitionStatistics> players =
                await _statisticsRepository.GetPlayerCompetitionStatisticsAsync(
                    request.CompetitionId,
                    HockeyStatisticsScope.PlayoffSeries,
                    playoffSeriesId: request.PlayoffSeriesId);

            IReadOnlyList<HockeyGoalieCompetitionStatistics> goalies =
                await _statisticsRepository.GetGoalieCompetitionStatisticsAsync(
                    request.CompetitionId,
                    HockeyStatisticsScope.PlayoffSeries,
                    playoffSeriesId: request.PlayoffSeriesId);

            return Result<HockeyPlayoffSeriesStatisticsDto>.Success(new HockeyPlayoffSeriesStatisticsDto
            {
                CompetitionId = request.CompetitionId,
                PlayoffSeriesId = request.PlayoffSeriesId,
                Teams = teams.Select(HockeyStatisticsMapper.ToDto).ToList(),
                Players = players.Select(HockeyStatisticsMapper.ToDto).ToList(),
                Goalies = goalies.Select(HockeyStatisticsMapper.ToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyPlayoffSeriesStatistics for {CompetitionId}", request.CompetitionId);
            return Result<HockeyPlayoffSeriesStatisticsDto>.Failure(
                "An error occurred while retrieving playoff series statistics.",
                ex.Flatten());
        }
    }
}
