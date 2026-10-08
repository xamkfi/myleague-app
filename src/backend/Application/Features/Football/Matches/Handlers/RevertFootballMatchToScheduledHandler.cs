using Application.Common;
using Application.Constants;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Puts a match that was started by mistake back to Scheduled and removes its timer, so the
/// next start begins from 00:00. The domain only allows this for a 0-0 match with no events,
/// so there are no statistics to undo.
/// </summary>
public class RevertFootballMatchToScheduledHandler
    : IRequestHandler<RevertFootballMatchToScheduledCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RevertFootballMatchToScheduledHandler> _logger;

    public RevertFootballMatchToScheduledHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        INotificationSenderService notificationSenderService,
        ILogger<RevertFootballMatchToScheduledHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _timerService = timerService;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(
        RevertFootballMatchToScheduledCommand request,
        CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
        if (match is null)
        {
            return Result<FootballMatchDto>.NotFound("FootballMatch", request.Id);
        }

        try
        {
            match.RevertToScheduled();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot revert match {MatchId} to scheduled", request.Id);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _timerService.DestroyTimerAsync(request.Id);
        await _notificationSenderService.SendNotificationAsync(
            FootballNotificationEvents.MatchRevertedToScheduled,
            new MatchNotificationPayload(match.Id));

        _logger.LogInformation("Reverted football match {MatchId} to scheduled", request.Id);
        return Result<FootballMatchDto>.Success(FootballMatchMapper.ToDto(match));
    }
}
