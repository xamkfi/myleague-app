using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Domain.Entities.Floorball.Matches;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for permanently deleting a floorball match. Statistics the match produced are reverted in
/// the same transaction. A playoff match can only be deleted before it starts.
/// </summary>
public class DeleteFloorballMatchHandler : IRequestHandler<DeleteFloorballMatchCommand, Result>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IMatchTimerService _timerService;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFloorballMatchHandler> _logger;

    public DeleteFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IMatchTimerService timerService,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeleteFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _timerService = timerService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
        {
            return Result.NotFound("FloorballMatch", request.MatchId);
        }

        // Bracket advancement is not rolled back, so a playoff match can only be removed before it starts.
        if (match.PlayoffRound.HasValue && match.Status != FloorballMatchStatus.Scheduled)
        {
            return Result.Failure("A playoff match that has been started cannot be deleted.");
        }

        await FloorballMatchStatisticsSupport.UndoMatchAsync(
            _statisticsRepository, _playerRepository, match, cancellationToken);
        await _matchRepository.DeleteAsync(match.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (match.Status == FloorballMatchStatus.InProgress)
        {
            await _timerService.DestroyTimerAsync(match.Id);
        }

        _logger.LogInformation(
            "Deleted floorball match {MatchId} with status {Status}", request.MatchId, match.Status);

        return Result.Success();
    }
}
