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
using System.Collections.Generic;
using Application.Mappings.Floorball;

namespace Application.Handlers.Floorball.Matches.Events;

/// <summary>
/// Handler for changing the date/time of an event-sourced floorball match
/// </summary>
public class ChangeEventSourcedFloorballMatchDateTimeHandler : IRequestHandler<ChangeEventSourcedFloorballMatchDateTimeCommand, Result<FloorballMatchDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly ILogger<ChangeEventSourcedFloorballMatchDateTimeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ChangeEventSourcedFloorballMatchDateTimeHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="logger">The logger</param>
    public ChangeEventSourcedFloorballMatchDateTimeHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        ILogger<ChangeEventSourcedFloorballMatchDateTimeHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ChangeEventSourcedFloorballMatchDateTimeCommand request
    /// </summary>
    /// <param name="request">The command containing match ID and new date/time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(ChangeEventSourcedFloorballMatchDateTimeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Changing date/time for event-sourced floorball match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Change the date/time using event sourcing (reschedule with same venue)
            match.Reschedule(request.NewDateTime, match.Venue);

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Create the DTO response
            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match, "Home Team", "Away Team");

            _logger.LogInformation("Successfully changed date/time for event-sourced floorball match: {MatchId}", request.MatchId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while changing date/time for match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing date/time for event-sourced floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while changing the date/time.");
        }
    }
} 