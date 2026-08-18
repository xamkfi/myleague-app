using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles DeactivateHockeyMatchLineCommand.
/// </summary>
public class DeactivateHockeyMatchLineHandler : IRequestHandler<DeactivateHockeyMatchLineCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateHockeyMatchLineHandler> _logger;

    public DeactivateHockeyMatchLineHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DeactivateHockeyMatchLineHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(DeactivateHockeyMatchLineCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(DeactivateHockeyMatchLineCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                HockeyMatchLine line = HockeyMatchHandlerSupport.GetRequiredMatchLine(matchTeam, request.MatchLineId);
                line.Deactivate();
            },
            cancellationToken);
}
