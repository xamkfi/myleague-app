using Application.Common;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Puts a match that was started by mistake back to Scheduled, deletes its period markers and
/// period scores, and removes its timer so the next start begins from 00:00.
/// </summary>
public class RevertHockeyMatchToScheduledHandler
    : IRequestHandler<RevertHockeyMatchToScheduledCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<RevertHockeyMatchToScheduledHandler> _logger;

    public RevertHockeyMatchToScheduledHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        ILogger<RevertHockeyMatchToScheduledHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _timerService = timerService;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        RevertHockeyMatchToScheduledCommand request,
        CancellationToken cancellationToken)
    {
        Result<HockeyMatchDto> result = await HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(RevertHockeyMatchToScheduledCommand),
            match =>
            {
                IReadOnlyList<HockeyMatchEvent> removedEvents = match.RevertToScheduled();
                foreach (HockeyMatchEvent removedEvent in removedEvents)
                {
                    _matchRepository.MarkEventAsDeleted(removedEvent);
                }
            },
            cancellationToken);

        if (result.IsSuccess)
        {
            await _timerService.DestroyTimerAsync(request.MatchId);
        }

        return result;
    }
}
