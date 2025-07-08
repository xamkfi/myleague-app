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
/// Handler for adding an official to an event-sourced floorball match
/// </summary>
public class AddOfficialToEventSourcedMatchHandler : IRequestHandler<AddOfficialToEventSourcedMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly ILogger<AddOfficialToEventSourcedMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the AddOfficialToEventSourcedMatchHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="refereeRepository">Repository for referees (for existence check)</param>
    /// <param name="logger">The logger</param>
    public AddOfficialToEventSourcedMatchHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        IFloorballRefereeRepository refereeRepository,
        ILogger<AddOfficialToEventSourcedMatchHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _refereeRepository = refereeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the AddOfficialToEventSourcedMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match and referee IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(AddOfficialToEventSourcedMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding official to event-sourced floorball match: {MatchId}, Official: {RefereeId}", 
                request.MatchId, request.RefereeId);

                        // Ensure referee exists
            bool refereeExists = await _refereeRepository.ExistsAsync(request.RefereeId);
            if (!refereeExists)
            {
                _logger.LogWarning("Referee with ID {RefereeId} not found.", request.RefereeId);
                return Result<FloorballMatchDto>.Failure($"Referee with ID {request.RefereeId} not found.");
            }
            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Add the official using event sourcing
            match.AddOfficial(request.RefereeId);

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Create the DTO response
            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match, "Home Team", "Away Team");

            _logger.LogInformation("Successfully added official to event-sourced floorball match: {MatchId}", request.MatchId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while adding official to match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding official to event-sourced floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while adding the official to the match.");
        }
    }
} 