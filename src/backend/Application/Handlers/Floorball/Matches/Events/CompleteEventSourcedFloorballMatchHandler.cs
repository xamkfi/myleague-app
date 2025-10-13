using Application.Commands.Floorball.MatchEvent;
using Application.DTOs.Floorball;
using Application.Common;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Mappings.Floorball;

namespace Application.Handlers.Floorball.Matches.Events;

/// <summary>
/// Handler for completing an event-sourced floorball match
/// </summary>
public class CompleteEventSourcedFloorballMatchHandler : IRequestHandler<CompleteEventSourcedFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<CompleteEventSourcedFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CompleteEventSourcedFloorballMatchHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="timerService">The timer service</param>
    /// <param name="logger">The logger</param>
    public CompleteEventSourcedFloorballMatchHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        IMatchTimerService timerService,
        ILogger<CompleteEventSourcedFloorballMatchHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _timerService = timerService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CompleteEventSourcedFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The completed match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(CompleteEventSourcedFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Completing event-sourced floorball match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Complete the match using event sourcing
            match.Complete();

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Destroy the timer for this match to prevent background service queries
            try
            {
                _logger.LogInformation("Destroying timer for completed match: {MatchId}", request.MatchId);
                await _timerService.DestroyTimerAsync(request.MatchId);
                _logger.LogInformation("Successfully destroyed timer for completed match: {MatchId}", request.MatchId);
            }
            catch (Exception timerEx)
            {
                _logger.LogWarning(timerEx, "Failed to destroy timer for completed match: {MatchId}. This is non-critical.", request.MatchId);
                // Don't fail the match completion if timer destruction fails
            }

            // Create the DTO response
            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match, "Home Team", "Away Team");

            _logger.LogInformation("Successfully completed event-sourced floorball match: {MatchId}", request.MatchId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while completing match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing event-sourced floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while completing the match.");
        }
    }
} 