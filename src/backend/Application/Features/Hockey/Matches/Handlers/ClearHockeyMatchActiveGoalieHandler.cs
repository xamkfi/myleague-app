using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles ClearHockeyMatchActiveGoalieCommand.
/// </summary>
public class ClearHockeyMatchActiveGoalieHandler : IRequestHandler<ClearHockeyMatchActiveGoalieCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<ClearHockeyMatchActiveGoalieHandler> _logger;

    public ClearHockeyMatchActiveGoalieHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<ClearHockeyMatchActiveGoalieHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(ClearHockeyMatchActiveGoalieCommand request, CancellationToken cancellationToken)
    {
        Result<HockeyMatchDto> result = await HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(ClearHockeyMatchActiveGoalieCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                matchTeam.ClearActiveGoalie();
            },
            cancellationToken);

        await HockeyMatchHandlerSupport.RecalculateStatisticsIfFinishedAsync(_mediator, _logger, result, cancellationToken);
        return result;
    }
}
