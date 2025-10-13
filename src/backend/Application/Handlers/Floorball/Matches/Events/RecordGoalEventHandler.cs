using Application.Commands.Floorball.MatchEvent;
using Application.DTOs.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches.Events;

/// <summary>
/// Handler for recording a goal event using event sourcing
/// </summary>
public class RecordGoalEventHandler : IRequestHandler<RecordGoalEventCommand, Result<FloorballGoalEventDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly ILogger<RecordGoalEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RecordGoalEventHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="logger">The logger</param>
    public RecordGoalEventHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        ILogger<RecordGoalEventHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RecordGoalEventCommand request
    /// </summary>
    /// <param name="request">The command containing goal event details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The recorded goal event as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballGoalEventDto>> Handle(RecordGoalEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recording goal event for match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Record the goal using event sourcing
            match.RecordGoal(
                request.TeamId,
                request.PlayerId,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.WasInOvertime,
                request.WasInShootout,
                request.AssisterId,
                request.SecondaryAssisterId);

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Create the DTO response
            FloorballGoalEventDto goalEventDto = new FloorballGoalEventDto(
                request.TeamId,
                request.PlayerId,
                request.AssisterId,
                request.SecondaryAssisterId,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.WasInOvertime,
                request.WasInShootout,
                "Unknown Player", // PlayerName - would need player lookup for actual name
                request.AssisterId.HasValue ? "Unknown Player" : null, // AssisterName
                request.SecondaryAssisterId.HasValue ? "Unknown Player" : null); // SecondaryAssisterName

            _logger.LogInformation("Successfully recorded goal event for match: {MatchId}", request.MatchId);

            return Result<FloorballGoalEventDto>.Success(goalEventDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballGoalEventDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while recording goal for match: {MatchId}", request.MatchId);
            return Result<FloorballGoalEventDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments while recording goal for match: {MatchId}", request.MatchId);
            return Result<FloorballGoalEventDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording goal event for match: {MatchId}", request.MatchId);
            return Result<FloorballGoalEventDto>.Failure("An error occurred while recording the goal event.");
        }
    }
} 
