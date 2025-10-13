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
/// Handler for recording shootout in an event-sourced floorball match
/// </summary>
public class RecordShootoutEventHandler : IRequestHandler<RecordShootoutEventCommand, Result<FloorballMatchDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly ILogger<RecordShootoutEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RecordShootoutEventHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="logger">The logger</param>
    public RecordShootoutEventHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        ILogger<RecordShootoutEventHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RecordShootoutEventCommand request
    /// </summary>
    /// <param name="request">The command containing match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(RecordShootoutEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recording shootout for event-sourced floorball match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Record shootout using event sourcing
            match.RecordShootout();

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Create the DTO response
            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match, "Home Team", "Away Team");

            _logger.LogInformation("Successfully recorded shootout for event-sourced floorball match: {MatchId}", request.MatchId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while recording shootout for match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording shootout for event-sourced floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while recording shootout for the match.");
        }
    }
} 