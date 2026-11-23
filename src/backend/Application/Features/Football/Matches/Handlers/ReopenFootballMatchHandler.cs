using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Application.Services.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class ReopenFootballMatchHandler : IRequestHandler<ReopenFootballMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<ReopenFootballMatchHandler> _logger;

    public ReopenFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<ReopenFootballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(ReopenFootballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FootballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            if (match.PlayoffRound.HasValue)
            {
                _logger.LogWarning(
                    "Refusing to reopen playoff match {MatchId} (round={Round}). Playoff propagation rollback is not supported.",
                    request.Id,
                    match.PlayoffRound);
                return Result<FootballMatchDto>.Failure(
                    "Playoff matches cannot be reopened automatically. Please contact an administrator if a playoff result was recorded by mistake.");
            }

            _logger.LogInformation("Reopening football match: {MatchId}", request.Id);

            await FootballMatchStatisticsSupport.UndoCompletionAsync(_statisticsRepository, match, cancellationToken);

            match.ReopenFromCompleted();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationSenderService.SendNotificationAsync(
                FootballNotificationEvents.MatchReopened,
                new MatchNotificationPayload(match.Id));

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully reopened football match: {MatchId}", request.Id);

            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot reopen match {MatchId}: {Message}", request.Id, ex.Message);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
    }
}
