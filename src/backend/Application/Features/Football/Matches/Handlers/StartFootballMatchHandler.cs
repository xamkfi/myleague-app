using Application.Common;
using Application.Constants;
using Application.Features.Common.MatchTimer.Services;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Application.Services.Common;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class StartFootballMatchHandler : IRequestHandler<StartFootballMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<StartFootballMatchHandler> _logger;

    public StartFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        IMatchTimerService timerService,
        ILogger<StartFootballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _timerService = timerService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(StartFootballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FootballMatchDto>.NotFound("FootballMatch", request.Id);
            }

            _logger.LogInformation("Starting football match: {MatchId}", request.Id);
            match.Start();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!await _timerService.ExistsAsync(match.Id))
            {
                _logger.LogInformation("Creating timer for match {MatchId}", match.Id);
                await _timerService.CreateTimerAsync(match.Id);
            }

            _logger.LogInformation("Starting timer for match {MatchId} period 1", match.Id);
            await _timerService.StartTimerAsync(match.Id, periodNumber: 1);

            await _notificationSenderService.SendNotificationAsync(
                FootballNotificationEvents.MatchStarted,
                new MatchNotificationPayload(match.Id));

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully started football match: {MatchId}", request.Id);

            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred while starting football match: {MatchId}", request.Id);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting football match: {MatchId}", request.Id);
            return Result<FootballMatchDto>.Failure("An error occurred while starting the match.");
        }
    }
}
