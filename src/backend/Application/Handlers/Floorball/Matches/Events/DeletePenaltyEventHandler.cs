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
/// Handler for deleting a penalty event from a floorball match
/// </summary>
public class DeletePenaltyEventHandler : IRequestHandler<DeletePenaltyEventCommand, Result<FloorballPenaltyEventDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePenaltyEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the DeletePenaltyEventHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public DeletePenaltyEventHandler(
        IFloorballMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeletePenaltyEventHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeletePenaltyEventCommand request
    /// </summary>
    /// <param name="request">The command containing the penalty event ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deleted penalty event as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballPenaltyEventDto>> Handle(DeletePenaltyEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting penalty event with ID: {PenaltyEventId}", request.Id);

            // Find which match contains this penalty event
            IEnumerable<FloorballMatch> allMatches = await _matchRepository.GetAllAsync();
            
            FloorballMatch? matchWithPenalty = null;
            FloorballPenalty? penaltyToDelete = null;
            
            foreach (FloorballMatch match in allMatches)
            {
                penaltyToDelete = match.PenaltyEvents.FirstOrDefault(p => p.Id == request.Id);
                if (penaltyToDelete != null)
                {
                    matchWithPenalty = match;
                    break;
                }
            }

            if (matchWithPenalty == null || penaltyToDelete == null)
            {
                _logger.LogWarning("Penalty event with ID {PenaltyEventId} not found", request.Id);
                return Result<FloorballPenaltyEventDto>.Failure("Penalty event not found");
            }

            // Create the DTO before deleting the penalty
            var penaltyEventDto = new FloorballPenaltyEventDto(
                penaltyToDelete.TeamId,
                penaltyToDelete.PlayerId,
                penaltyToDelete.PenaltyType,
                penaltyToDelete.DurationInMinutes,
                penaltyToDelete.PeriodNumber,
                penaltyToDelete.TimeInSeconds,
                penaltyToDelete.Description ?? string.Empty
            );

            // Delete the penalty event from the match
            FloorballPenalty deletedPenalty = matchWithPenalty.DeletePenaltyEvent(request.Id);

            // Update the match in the repository
            await _matchRepository.UpdateAsync(matchWithPenalty);
            
            // Save changes to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted penalty event with ID: {PenaltyEventId} from match: {MatchId}", 
                request.Id, matchWithPenalty.Id);

            return Result<FloorballPenaltyEventDto>.Success(penaltyEventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting penalty event with ID: {PenaltyEventId}", request.Id);
            return Result<FloorballPenaltyEventDto>.Failure("An error occurred while deleting the penalty event.");
        }
    }
} 