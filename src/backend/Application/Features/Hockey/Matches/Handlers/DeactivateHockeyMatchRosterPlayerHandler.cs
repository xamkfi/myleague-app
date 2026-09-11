using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles DeactivateHockeyMatchRosterPlayerCommand.
/// </summary>
public class DeactivateHockeyMatchRosterPlayerHandler : IRequestHandler<DeactivateHockeyMatchRosterPlayerCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateHockeyMatchRosterPlayerHandler> _logger;

    public DeactivateHockeyMatchRosterPlayerHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<DeactivateHockeyMatchRosterPlayerHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(DeactivateHockeyMatchRosterPlayerCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(DeactivateHockeyMatchRosterPlayerCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                if (matchTeam.PlayerSelection is null)
                {
                    throw new InvalidOperationException("Match team has no player selection.");
                }

                matchTeam.PlayerSelection.DeactivatePlayer(request.MatchActivePlayerId);
            },
            cancellationToken);
}
