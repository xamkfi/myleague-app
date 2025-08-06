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
/// Handler for recording a save event using event sourcing
/// </summary>
public class RecordSaveEventHandler : IRequestHandler<RecordSaveEventCommand, Result<FloorballSaveEventDto>>
{
    private readonly IEventSourcedFloorballMatchRepository _eventSourcedMatchRepository;
    private readonly ILogger<RecordSaveEventHandler> _logger;

    public RecordSaveEventHandler(
        IEventSourcedFloorballMatchRepository eventSourcedMatchRepository,
        ILogger<RecordSaveEventHandler> logger)
    {
        _eventSourcedMatchRepository = eventSourcedMatchRepository;
        _logger = logger;
    }

    public async Task<Result<FloorballSaveEventDto>> Handle(RecordSaveEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Recording save event for match: {MatchId}", request.MatchId);

            // Get the event sourced match
            EventSourcedFloorballMatch match = await _eventSourcedMatchRepository.GetByIdAsync(request.MatchId, cancellationToken);

            // Record the save using event sourcing
            match.RecordSave(
                request.TeamId,
                request.GoalieId,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.WasInOvertime,
                request.WasInShootout);

            // Save the match with its new events
            await _eventSourcedMatchRepository.SaveAsync(match, cancellationToken);

            // Create the DTO response
            var saveEventDto = new FloorballSaveEventDto
            {
                TeamId = request.TeamId,
                GoalieId = request.GoalieId,
                PeriodNumber = request.PeriodNumber,
                TimeInSeconds = request.TimeInSeconds,
                WasInOvertime = request.WasInOvertime,
                WasInShootout = request.WasInShootout,
                GoalieName = "Unknown Goalie" // Would need player lookup for actual name
            };

            _logger.LogInformation("Successfully recorded save event for match: {MatchId}", request.MatchId);

            return Result<FloorballSaveEventDto>.Success(saveEventDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Match not found: {MatchId}", request.MatchId);
            return Result<FloorballSaveEventDto>.Failure($"Match with ID {request.MatchId} not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while recording save for match: {MatchId}", request.MatchId);
            return Result<FloorballSaveEventDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments while recording save for match: {MatchId}", request.MatchId);
            return Result<FloorballSaveEventDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording save event for match: {MatchId}", request.MatchId);
            return Result<FloorballSaveEventDto>.Failure("An error occurred while recording the save event.");
        }
    }
}
