using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles SetHockeyMatchActiveGoalieCommand.
/// </summary>
public class SetHockeyMatchActiveGoalieHandler : IRequestHandler<SetHockeyMatchActiveGoalieCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeyMatchActiveGoalieHandler> _logger;

    public SetHockeyMatchActiveGoalieHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeyMatchActiveGoalieHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(SetHockeyMatchActiveGoalieCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(SetHockeyMatchActiveGoalieCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                HockeyMatchActivePlayer player = HockeyMatchHandlerSupport.GetRequiredActivePlayer(matchTeam, request.MatchActivePlayerId);
                matchTeam.SetActiveGoalie(player);
            },
            cancellationToken);
}
