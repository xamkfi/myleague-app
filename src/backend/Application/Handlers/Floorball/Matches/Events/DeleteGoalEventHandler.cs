using Application.Commands.Floorball.MatchEvent;
using Application.DTOs.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches.Events;

/// <summary>
/// Handler for deleting a goal event from a floorball match
/// </summary>
public class DeleteGoalEventHandler : IRequestHandler<DeleteGoalEventCommand, Result<FloorballGoalEventDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGoalEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteGoalEventHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeleteGoalEventHandler(
        IFloorballMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteGoalEventHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteGoalEventCommand request
    /// </summary>
    /// <param name="request">The command containing the goal event ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deleted goal event as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballGoalEventDto>> Handle(DeleteGoalEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting goal event with ID: {GoalEventId}", request.Id);

            // First, we need to find which match contains this goal event
            // Since we don't have a direct repository for goal events, we need to search through matches
            // This is not the most efficient approach, but it works with the current architecture
            
            // For now, let's assume we can find the match by checking all matches
            // In a real-world scenario, you might want to add a direct repository for match events
            // or store the match ID in the goal event
            
            // This is a simplified approach - in practice, you'd want to optimize this
            IEnumerable<FloorballMatch> allMatches = await _matchRepository.GetAllAsync();
            
            FloorballMatch? matchWithGoal = null;
            FloorballGoal? goalToDelete = null;
            
            foreach (FloorballMatch match in allMatches)
            {
                goalToDelete = match.GoalEvents.FirstOrDefault(g => g.Id == request.Id);
                if (goalToDelete != null)
                {
                    matchWithGoal = match;
                    break;
                }
            }

            if (matchWithGoal == null || goalToDelete == null)
            {
                _logger.LogWarning("Goal event with ID {GoalEventId} not found", request.Id);
                return Result<FloorballGoalEventDto>.Failure("Goal event not found");
            }

            // Create the DTO before deleting the goal
            FloorballGoalEventDto goalEventDto = new FloorballGoalEventDto(
                goalToDelete.TeamId,
                goalToDelete.ScoringPlayerId ?? Guid.Empty,
                goalToDelete.AssistingPlayerId,
                goalToDelete.SecondaryAssistingPlayerId,
                goalToDelete.PeriodNumber,
                goalToDelete.TimeInSeconds,
                false, // WasInOvertime - this information might not be available in the current structure
                false, // WasInShootout - this information might not be available in the current structure
                "Unknown Player", // PlayerName - would need player lookup for actual name
                goalToDelete.AssistingPlayerId.HasValue ? "Unknown Player" : null, // AssisterName
                goalToDelete.SecondaryAssistingPlayerId.HasValue ? "Unknown Player" : null); // SecondaryAssisterName

            // Delete the goal event from the match
            FloorballGoal deletedGoal = matchWithGoal.DeleteGoalEvent(request.Id);

            // Update the match in the repository
            await _matchRepository.UpdateAsync(matchWithGoal);
            
            // Save changes to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted goal event with ID: {GoalEventId} from match: {MatchId}", 
                request.Id, matchWithGoal.Id);

            return Result<FloorballGoalEventDto>.Success(goalEventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting goal event with ID: {GoalEventId}", request.Id);
            return Result<FloorballGoalEventDto>.Failure("An error occurred while deleting the goal event.");
        }
    }
} 
