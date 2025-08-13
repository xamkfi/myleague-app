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

/// <summary>
/// Handler for starting a period in a floorball match
/// </summary>
public class StartPeriodHandler : IRequestHandler<StartPeriodCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<StartPeriodHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the StartPeriodHandler class
    /// </summary>
    public StartPeriodHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<StartPeriodHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the StartPeriodCommand request
    /// </summary>
    public async Task<Result<FloorballMatchDto>> Handle(StartPeriodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            match.StartPeriod(request.PeriodNumber);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting period {Period} for match {MatchId}", request.PeriodNumber, request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while starting the period.");
        }
    }
}


