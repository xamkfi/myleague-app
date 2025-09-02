using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for deleting a penalty event from a floorball match (non-event-sourced)
/// </summary>
public class DeletePenaltyHandler : IRequestHandler<DeletePenaltyCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePenaltyHandler> _logger;

    public DeletePenaltyHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<DeletePenaltyHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(DeletePenaltyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            _logger.LogInformation("Deleting penalty {PenaltyId} from match {MatchId}", request.PenaltyEventId, request.MatchId);
            match.DeletePenaltyEvent(request.PenaltyEventId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting penalty {PenaltyId} from match {MatchId}", request.PenaltyEventId, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while deleting the penalty.");
        }
    }
}


