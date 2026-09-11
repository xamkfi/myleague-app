using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles admin correction of team goals on a hockey match.
/// </summary>
public class SetHockeyMatchTeamGoalsHandler : IRequestHandler<SetHockeyMatchTeamGoalsCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeyMatchTeamGoalsHandler> _logger;

    public SetHockeyMatchTeamGoalsHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeyMatchTeamGoalsHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(SetHockeyMatchTeamGoalsCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(SetHockeyMatchTeamGoalsCommand),
            match => match.SetTeamGoals(request.TeamSlot, request.Goals),
            cancellationToken);
}
