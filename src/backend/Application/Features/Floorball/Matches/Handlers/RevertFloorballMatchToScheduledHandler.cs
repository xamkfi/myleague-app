using Application.Common;
using Application.Constants;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Floorball.Matches;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Puts a match that was started by mistake back to Scheduled and removes its timer, so the
/// next start begins from 00:00. The domain only allows this for a 0-0 match with no events,
/// so there are no statistics to undo.
/// </summary>
public class RevertFloorballMatchToScheduledHandler
    : IRequestHandler<RevertFloorballMatchToScheduledCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<RevertFloorballMatchToScheduledHandler> _logger;

    public RevertFloorballMatchToScheduledHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        INotificationSenderService notificationSenderService,
        ILogger<RevertFloorballMatchToScheduledHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _timerService = timerService;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(
        RevertFloorballMatchToScheduledCommand request,
        CancellationToken cancellationToken)
    {
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
        if (match is null)
        {
            return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.Id);
        }

        try
        {
            match.RevertToScheduled();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot revert match {MatchId} to scheduled", request.Id);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _timerService.DestroyTimerAsync(request.Id);
        await _notificationSenderService.SendNotificationAsync(
            FloorballNotificationEvents.MatchRevertedToScheduled,
            new MatchNotificationPayload(match.Id));

        _logger.LogInformation("Reverted floorball match {MatchId} to scheduled", request.Id);
        return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
    }
}
