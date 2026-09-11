using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles DisableHockeyMatchOnIceTrackingCommand.
/// </summary>
public class DisableHockeyMatchOnIceTrackingHandler : IRequestHandler<DisableHockeyMatchOnIceTrackingCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DisableHockeyMatchOnIceTrackingHandler> _logger;

    public DisableHockeyMatchOnIceTrackingHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DisableHockeyMatchOnIceTrackingHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(DisableHockeyMatchOnIceTrackingCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(DisableHockeyMatchOnIceTrackingCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                matchTeam.DisableOnIceTracking(request.UserId);
            },
            cancellationToken);
}
