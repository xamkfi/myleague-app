using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles creating a period score row for a hockey match.
/// </summary>
public class AddHockeyPeriodScoreHandler : IRequestHandler<AddHockeyPeriodScoreCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHockeyPeriodScoreHandler> _logger;

    public AddHockeyPeriodScoreHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHockeyPeriodScoreHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(AddHockeyPeriodScoreCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(AddHockeyPeriodScoreCommand),
            match => match.AddPeriodScore(request.PeriodNumber, request.PeriodType),
            cancellationToken);
}
