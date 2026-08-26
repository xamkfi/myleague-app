using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles marking a hockey match as finished.
/// </summary>
public class MarkHockeyMatchFinishedHandler : IRequestHandler<MarkHockeyMatchFinishedCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<MarkHockeyMatchFinishedHandler> _logger;

    public MarkHockeyMatchFinishedHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<MarkHockeyMatchFinishedHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(MarkHockeyMatchFinishedCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(MarkHockeyMatchFinishedCommand),
            match => match.MarkFinished(DateTimeUtc.Normalize(request.ActualEndTime), request.ResultType),
            cancellationToken);
}
