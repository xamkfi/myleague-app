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

public class RecordOvertimeHandler : IRequestHandler<RecordOvertimeCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RecordOvertimeHandler> _logger;

    public RecordOvertimeHandler(IFloorballMatchRepository matchRepository, IFloorballUnitOfWork unitOfWork, ILogger<RecordOvertimeHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RecordOvertimeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.RecordOvertime();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording overtime for match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while recording overtime.");
        }
    }
}


