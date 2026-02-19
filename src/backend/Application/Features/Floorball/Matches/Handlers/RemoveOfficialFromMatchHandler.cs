using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Matches;

public class RemoveOfficialFromMatchHandler : IRequestHandler<RemoveOfficialFromMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveOfficialFromMatchHandler> _logger;

    public RemoveOfficialFromMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RemoveOfficialFromMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RemoveOfficialFromMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.RemoveOfficial(request.RefereeId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException inv)
        {
            _logger.LogWarning(inv, "Validation error removing official {RefereeId} from match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(inv.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing official {RefereeId} from match {MatchId}", request.RefereeId, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while removing the official.");
        }
    }
}

