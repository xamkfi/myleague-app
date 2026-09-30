using Application.Common;
using Application.Features.Common.CrossCutting.MatchTimer.Services;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Domain.Entities.Floorball.Matches;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for ending a period in a floorball match. Also stops the live match timer so the
/// clock cannot keep running after the period has been closed.
/// </summary>
public class EndPeriodHandler : IRequestHandler<EndFloorballPeriodCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<EndPeriodHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the EndPeriodHandler class
    /// </summary>
    public EndPeriodHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        ILogger<EndPeriodHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _timerService = timerService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the EndFloorballPeriodCommand request
    /// </summary>
    public async Task<Result<FloorballMatchDto>> Handle(EndFloorballPeriodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.NotFound("Match", request.MatchId);
            }

            match.EndPeriod(request.PeriodNumber);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // The period is closed; freeze the clock so the next period starts from a paused
            // timer. StopTimerAsync is a no-op when no timer exists for the match.
            await _timerService.StopTimerAsync(match.Id);

            return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Period {Period} for match {MatchId} could not be ended: {Reason}", request.PeriodNumber, request.MatchId, ex.Message);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid period {Period} for match {MatchId}: {Reason}", request.PeriodNumber, request.MatchId, ex.Message);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }
    }
}
