using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using Domain.Entities.Floorball;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// ReactivateMatchHandler is responsible for handling the reactivation of a cancelled floorball match.
/// </summary>
public class ReactivateMatchHandler : IRequestHandler<ReactivateMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<ReactivateMatchHandler> _logger;

    public ReactivateMatchHandler(IFloorballMatchRepository matchRepository, IFloorballUnitOfWork unitOfWork, ILogger<ReactivateMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(ReactivateMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.Reactivate();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot reactivate match {MatchId}: {Message}", request.MatchId, ex.Message);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reactivating match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while reactivating the match.");
        }
    }
}
