using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles removing an official from a hockey match.
/// </summary>
public class RemoveHockeyMatchOfficialHandler
    : IRequestHandler<RemoveHockeyMatchOfficialCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveHockeyMatchOfficialHandler> _logger;

    public RemoveHockeyMatchOfficialHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveHockeyMatchOfficialHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(
        RemoveHockeyMatchOfficialCommand request,
        CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(RemoveHockeyMatchOfficialCommand),
            match => match.RemoveOfficial(request.OfficialId),
            cancellationToken);
}
