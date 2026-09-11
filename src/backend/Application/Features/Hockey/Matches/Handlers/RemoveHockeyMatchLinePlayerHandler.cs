using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles RemoveHockeyMatchLinePlayerCommand.
/// </summary>
public class RemoveHockeyMatchLinePlayerHandler : IRequestHandler<RemoveHockeyMatchLinePlayerCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveHockeyMatchLinePlayerHandler> _logger;

    public RemoveHockeyMatchLinePlayerHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveHockeyMatchLinePlayerHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(RemoveHockeyMatchLinePlayerCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(RemoveHockeyMatchLinePlayerCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                HockeyMatchLine line = HockeyMatchHandlerSupport.GetRequiredMatchLine(matchTeam, request.MatchLineId);
                line.RemovePlayer(request.MatchActivePlayerId);
            },
            cancellationToken);
}
