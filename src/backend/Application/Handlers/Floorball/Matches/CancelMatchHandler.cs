using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Handlers.Floorball.Matches;

public class CancelMatchHandler : IRequestHandler<CancelMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CancelMatchHandler> _logger;

    public CancelMatchHandler(IFloorballMatchRepository matchRepository, IFloorballUnitOfWork unitOfWork, ILogger<CancelMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(CancelMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.Cancel();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while canceling match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while canceling the match.");
        }
    }
}


