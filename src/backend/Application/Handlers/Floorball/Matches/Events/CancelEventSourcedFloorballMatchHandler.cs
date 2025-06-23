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

namespace Application.Handlers.Floorball.Matches.Events;

/// <summary>
/// Handler for canceling an event-sourced floorball match
/// </summary>
public class CancelEventSourcedFloorballMatchHandler : IRequestHandler<CancelEventSourcedFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly ILogger<CancelEventSourcedFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CancelEventSourcedFloorballMatchHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="logger">The logger</param>
    public CancelEventSourcedFloorballMatchHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        ILogger<CancelEventSourcedFloorballMatchHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CancelEventSourcedFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The canceled match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(CancelEventSourcedFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Canceling event-sourced floorball match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Cancel the match using event sourcing
            match.Cancel();

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Create the DTO response
            var matchDto = new FloorballMatchDto(
                match.Id,
                match.SeasonId,
                match.HomeTeamId,
                "Home Team", // Placeholder - in full implementation would fetch from repository
                match.AwayTeamId,
                "Away Team", // Placeholder - in full implementation would fetch from repository
                match.ScheduledDateTime,
                match.Venue,
                match.Status,
                match.HomeScore,
                match.AwayScore,
                match.WentToOvertime,
                match.WentToShootout,
                match.PeriodScores,
                match.OfficialIds,
                new List<FloorballGoalEventDto>(), // Empty for now
                new List<FloorballPenaltyEventDto>() // Empty for now
            );

            _logger.LogInformation("Successfully canceled event-sourced floorball match: {MatchId}", request.MatchId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while canceling match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while canceling event-sourced floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while canceling the match.");
        }
    }
} 