using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Common;
using Application.Features.Common.MatchTimer.Services;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;
using Application.Interfaces.Common;
using Application.Constants;
using Application.Services.Common;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for starting a floorball match
/// </summary>
public class StartFloorballMatchHandler : IRequestHandler<StartFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<StartFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the StartFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="notificationSenderService">The notification sender service</param>
    /// <param name="timerService">The match timer service</param>
    /// <param name="logger">The logger</param>
    public StartFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        IMatchTimerService timerService,
        ILogger<StartFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _timerService = timerService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the StartFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The started match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(StartFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the match
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            _logger.LogInformation("Starting floorball match: {MatchId}", request.Id);
            match.Start();
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Auto-create and start timer for period 1
            if (!await _timerService.ExistsAsync(match.Id))
            {
                _logger.LogInformation("Creating timer for match {MatchId}", match.Id);
                await _timerService.CreateTimerAsync(match.Id);
            }
            
            _logger.LogInformation("Starting timer for match {MatchId} period 1", match.Id);
            await _timerService.StartTimerAsync(match.Id, periodNumber: 1);
            _logger.LogInformation("Timer auto-started for match {MatchId}", match.Id);

            await _notificationSenderService.SendNotificationAsync(
                FloorballNotificationEvents.MatchStarted,
                 new { MatchId = match.Id });

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully started floorball match: {MatchId}", request.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred while starting floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while starting the match.");
        }
    }
} 
