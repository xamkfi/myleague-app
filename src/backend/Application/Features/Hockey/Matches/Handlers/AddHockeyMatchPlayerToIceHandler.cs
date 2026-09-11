using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles AddHockeyMatchPlayerToIceCommand.
/// </summary>
public class AddHockeyMatchPlayerToIceHandler : IRequestHandler<AddHockeyMatchPlayerToIceCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHockeyMatchPlayerToIceHandler> _logger;

    public AddHockeyMatchPlayerToIceHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHockeyMatchPlayerToIceHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(AddHockeyMatchPlayerToIceCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(AddHockeyMatchPlayerToIceCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                if (matchTeam.OnIceState is null)
                {
                    matchTeam.EnableOnIceTracking(request.UserId);
                }

                HockeyMatchActivePlayer player = HockeyMatchHandlerSupport.GetRequiredActivePlayer(matchTeam, request.MatchActivePlayerId);
                TimeSpan? gameTime = request.TimeInSeconds is int seconds ? TimeSpan.FromSeconds(seconds) : null;
                matchTeam.OnIceState!.AddPlayerToIce(
                    player,
                    request.Slot,
                    request.Order,
                    request.IsGoalie,
                    request.IsExtraAttacker,
                    request.PeriodNumber,
                    gameTime,
                    request.UserId);
            },
            cancellationToken);
}
