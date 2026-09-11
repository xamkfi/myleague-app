using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles setting whether a hockey match went to shootout.
/// </summary>
public class SetHockeyMatchWentToShootoutHandler : IRequestHandler<SetHockeyMatchWentToShootoutCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<SetHockeyMatchWentToShootoutHandler> _logger;

    public SetHockeyMatchWentToShootoutHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<SetHockeyMatchWentToShootoutHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(SetHockeyMatchWentToShootoutCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(SetHockeyMatchWentToShootoutCommand),
            match => match.SetWentToShootout(request.WentToShootout),
            cancellationToken);
}
