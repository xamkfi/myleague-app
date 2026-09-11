using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles ApplyHockeyMatchLineToIceCommand.
/// </summary>
public class ApplyHockeyMatchLineToIceHandler : IRequestHandler<ApplyHockeyMatchLineToIceCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<ApplyHockeyMatchLineToIceHandler> _logger;

    public ApplyHockeyMatchLineToIceHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<ApplyHockeyMatchLineToIceHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<HockeyMatchDto>> Handle(ApplyHockeyMatchLineToIceCommand request, CancellationToken cancellationToken) =>
        HockeyMatchHandlerSupport.MutateAsync(
            _matchRepository,
            _unitOfWork,
            _logger,
            request.MatchId,
            nameof(ApplyHockeyMatchLineToIceCommand),
            match =>
            {
                HockeyMatchTeam matchTeam = HockeyMatchHandlerSupport.GetRequiredMatchTeam(match, request.MatchTeamId);
                if (matchTeam.OnIceState is null)
                {
                    matchTeam.EnableOnIceTracking(request.UserId);
                }

                HockeyMatchLine line = HockeyMatchHandlerSupport.GetRequiredMatchLine(matchTeam, request.MatchLineId);
                TimeSpan? gameTime = request.TimeInSeconds is int seconds ? TimeSpan.FromSeconds(seconds) : null;
                matchTeam.OnIceState!.ApplyLine(line, request.PeriodNumber, gameTime, request.UserId);
            },
            cancellationToken);
}
