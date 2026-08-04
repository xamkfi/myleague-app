using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles RemoveHockeyMatchPlayerFromIceCommand.
/// </summary>
public class RemoveHockeyMatchPlayerFromIceHandler : IRequestHandler<RemoveHockeyMatchPlayerFromIceCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveHockeyMatchPlayerFromIceHandler> _logger;

    public RemoveHockeyMatchPlayerFromIceHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveHockeyMatchPlayerFromIceHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(RemoveHockeyMatchPlayerFromIceCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(RemoveHockeyMatchPlayerFromIceCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                if (matchTeam.OnIceState is null)
                {
                    throw new InvalidOperationException("On-ice tracking is not enabled for this match team.");
                }

                TimeSpan? gameTime = request.TimeInSeconds is int seconds ? TimeSpan.FromSeconds(seconds) : null;
                matchTeam.OnIceState.RemovePlayerFromIce(
                    request.MatchActivePlayerId,
                    request.PeriodNumber,
                    gameTime,
                    request.UserId);
            },
            cancellationToken);
}
