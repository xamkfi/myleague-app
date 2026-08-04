using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles updating a hockey match scheduled start time.
/// </summary>
public class UpdateHockeyMatchScheduledStartHandler
    : IRequestHandler<UpdateHockeyMatchScheduledStartCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyMatchScheduledStartHandler> _logger;

    public UpdateHockeyMatchScheduledStartHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyMatchScheduledStartHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(
        UpdateHockeyMatchScheduledStartCommand request,
        CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(UpdateHockeyMatchScheduledStartCommand),
            match => match.UpdateScheduledStartTime(request.ScheduledStartTime),
            cancellationToken);
}
