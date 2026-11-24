using Application.Common;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Football.Matches.Commands;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class DeleteFootballMatchHandler : IRequestHandler<DeleteFootballMatchCommand, Result>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IMatchTimerService _timerService;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFootballMatchHandler> _logger;

    public DeleteFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballPlayerRepository playerRepository,
        IFootballStatisticsRepository statisticsRepository,
        IMatchTimerService timerService,
        IFootballUnitOfWork unitOfWork,
        ILogger<DeleteFootballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _timerService = timerService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFootballMatchCommand request, CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
        {
            return Result.NotFound("FootballMatch", request.MatchId);
        }

        // Bracket advancement is not rolled back, so a playoff match can only be removed before it starts.
        if (match.PlayoffRound.HasValue && match.Status != FootballMatchStatus.Scheduled)
        {
            return Result.Failure("A playoff match that has been started cannot be deleted.");
        }

        await FootballMatchStatisticsSupport.UndoMatchAsync(
            _statisticsRepository, _playerRepository, match, cancellationToken);
        await _matchRepository.DeleteAsync(match.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (match.Status == FootballMatchStatus.InProgress)
        {
            await _timerService.DestroyTimerAsync(match.Id);
        }

        _logger.LogInformation(
            "Deleted football match {MatchId} with status {Status}", request.MatchId, match.Status);

        return Result.Success();
    }
}
