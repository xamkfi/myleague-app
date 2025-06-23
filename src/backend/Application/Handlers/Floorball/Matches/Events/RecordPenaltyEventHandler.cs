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
/// Handler for recording a penalty event using event sourcing
/// </summary>
public class RecordPenaltyEventHandler : IRequestHandler<RecordPenaltyEventCommand, Result<FloorballPenaltyEventDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly ILogger<RecordPenaltyEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RecordPenaltyEventHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="logger">The logger</param>
    public RecordPenaltyEventHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        ILogger<RecordPenaltyEventHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RecordPenaltyEventCommand request
    /// </summary>
    /// <param name="request">The command containing penalty event details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The recorded penalty event as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballPenaltyEventDto>> Handle(RecordPenaltyEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recording penalty event for match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Record the penalty using event sourcing
            match.RecordPenalty(
                request.TeamId,
                request.PlayerId,
                request.PenaltyType,
                request.Minutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description);

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Create the DTO response
            var penaltyEventDto = new FloorballPenaltyEventDto(
                request.TeamId,
                request.PlayerId,
                request.PenaltyType,
                request.Minutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description);

            _logger.LogInformation("Successfully recorded penalty event for match: {MatchId}", request.MatchId);

            return Result<FloorballPenaltyEventDto>.Success(penaltyEventDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballPenaltyEventDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while recording penalty for match: {MatchId}", request.MatchId);
            return Result<FloorballPenaltyEventDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments while recording penalty for match: {MatchId}", request.MatchId);
            return Result<FloorballPenaltyEventDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording penalty event for match: {MatchId}", request.MatchId);
            return Result<FloorballPenaltyEventDto>.Failure("An error occurred while recording the penalty event.");
        }
    }
} 
