using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles AddHockeyMatchLinePlayerCommand.
/// </summary>
public class AddHockeyMatchLinePlayerHandler : IRequestHandler<AddHockeyMatchLinePlayerCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHockeyMatchLinePlayerHandler> _logger;

    public AddHockeyMatchLinePlayerHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHockeyMatchLinePlayerHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(AddHockeyMatchLinePlayerCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(AddHockeyMatchLinePlayerCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                HockeyMatchLine line = HockeyMatchHandlerSupport.GetRequiredMatchLine(matchTeam, request.MatchLineId);
                HockeyMatchActivePlayer player = HockeyMatchHandlerSupport.GetRequiredActivePlayer(matchTeam, request.MatchActivePlayerId);
                line.AddPlayer(player, request.Slot, request.Order);
            },
            cancellationToken);
}
