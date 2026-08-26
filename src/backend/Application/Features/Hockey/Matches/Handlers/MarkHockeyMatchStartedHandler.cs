using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles marking a hockey match as started.
/// </summary>
public class MarkHockeyMatchStartedHandler : IRequestHandler<MarkHockeyMatchStartedCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<MarkHockeyMatchStartedHandler> _logger;

    public MarkHockeyMatchStartedHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<MarkHockeyMatchStartedHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(MarkHockeyMatchStartedCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(MarkHockeyMatchStartedCommand),
            match => match.MarkStarted(DateTimeUtc.Normalize(request.ActualStartTime)),
            cancellationToken);
}
