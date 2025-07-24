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
/// Handler for changing the venue of an event-sourced floorball match
/// </summary>
public class ChangeEventSourcedFloorballMatchVenueHandler : IRequestHandler<ChangeEventSourcedFloorballMatchVenueCommand, Result<FloorballMatchDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<ChangeEventSourcedFloorballMatchVenueHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ChangeEventSourcedFloorballMatchVenueHandler class
    /// </summary>
    /// <param name="eventSourcedMatchRepository">The event sourced match repository</param>
    /// <param name="teamRepository">The team repository</param>
    /// <param name="logger">The logger</param>
    public ChangeEventSourcedFloorballMatchVenueHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        IFloorballTeamRepository teamRepository,
        ILogger<ChangeEventSourcedFloorballMatchVenueHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ChangeEventSourcedFloorballMatchVenueCommand request
    /// </summary>
    /// <param name="request">The command containing match ID and new venue</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(ChangeEventSourcedFloorballMatchVenueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Changing venue for event-sourced floorball match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Change the venue using event sourcing
            match.ChangeVenue(request.NewVenue);

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Fetch the actual team names
            FloorballTeam? homeTeam = await _teamRepository.GetByIdAsync(match.HomeTeamId);
            FloorballTeam? awayTeam = await _teamRepository.GetByIdAsync(match.AwayTeamId);

            // Create the DTO response with actual team names
            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match, 
                homeTeam?.Name ?? "Unknown Home Team", 
                awayTeam?.Name ?? "Unknown Away Team");

            _logger.LogInformation("Successfully changed venue for event-sourced floorball match: {MatchId}", request.MatchId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while changing venue for match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while changing venue for match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing venue for event-sourced floorball match: {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while changing the venue.");
        }
    }
} 