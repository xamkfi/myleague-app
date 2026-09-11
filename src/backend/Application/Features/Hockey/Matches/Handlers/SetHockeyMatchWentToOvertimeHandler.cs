using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles setting whether a hockey match went to overtime.
/// </summary>
public class SetHockeyMatchWentToOvertimeHandler : IRequestHandler<SetHockeyMatchWentToOvertimeCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeyMatchWentToOvertimeHandler> _logger;

    public SetHockeyMatchWentToOvertimeHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeyMatchWentToOvertimeHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(SetHockeyMatchWentToOvertimeCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(SetHockeyMatchWentToOvertimeCommand),
            match => match.SetWentToOvertime(request.WentToOvertime),
            cancellationToken);
}
