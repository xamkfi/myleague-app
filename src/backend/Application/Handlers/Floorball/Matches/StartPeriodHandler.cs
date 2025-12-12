using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Matches
{
    /// <summary>
    /// Handler for starting a period in a floorball match
    /// </summary>
    public class StartPeriodHandler : IRequestHandler<StartPeriodCommand, Result<FloorballMatchDto>>
    {
        private readonly IFloorballMatchRepository _matchRepository;
        private readonly IFloorballUnitOfWork _unitOfWork;
        private readonly IMatchTimerService _timerService;
        private readonly ILogger<StartPeriodHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the StartPeriodHandler class
        /// </summary>
        public StartPeriodHandler(
            IFloorballMatchRepository matchRepository,
            IFloorballUnitOfWork unitOfWork,
            IMatchTimerService timerService,
            ILogger<StartPeriodHandler> logger)
        {
            _matchRepository = matchRepository;
            _unitOfWork = unitOfWork;
            _timerService = timerService;
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

                // Validate match is in progress
                if (match.Status != Domain.Enums.Floorball.FloorballMatchStatus.InProgress)
                {
                    _logger.LogWarning("Cannot start period for match {MatchId} with status {Status}", request.MatchId, match.Status);
                    return Result<FloorballMatchDto>.Failure($"Match must be in progress to start a period. Current status: {match.Status}");
                }

                // Validate period number
                if (request.PeriodNumber < 1 || request.PeriodNumber > 4)
                {
                    _logger.LogWarning("Invalid period number {PeriodNumber} for match {MatchId}", request.PeriodNumber, request.MatchId);
                    return Result<FloorballMatchDto>.Failure($"Period number must be between 1 and 4. Received: {request.PeriodNumber}");
                }

                // Ensure period score exists for this period
                // Periods 1 and 2 are created at match creation
                // Periods 3 (OT) and 4 (Shootout) are created by RecordOvertime/RecordShootout
                if (!match.PeriodScores.Any(ps => ps.PeriodNumber == request.PeriodNumber))
                {
                    _logger.LogWarning("Period score not initialized for period {PeriodNumber} in match {MatchId}", request.PeriodNumber, request.MatchId);
                    return Result<FloorballMatchDto>.Failure($"Period {request.PeriodNumber} has not been initialized. For overtime/shootout, call the appropriate record endpoint first.");
                }

                _logger.LogInformation("Starting period {PeriodNumber} for match {MatchId}", request.PeriodNumber, request.MatchId);

                // Reset and start timer for new period
                // The timer service will automatically reset if the period changed
                await _timerService.StartTimerAsync(match.Id, request.PeriodNumber);
                
                _logger.LogInformation("Successfully started period {PeriodNumber} for match {MatchId}", request.PeriodNumber, request.MatchId);

                return Result<FloorballMatchDto>.Success(FloorballMatchMapper.ToDto(match));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting period {PeriodNumber} for match {MatchId}", request.PeriodNumber, request.MatchId);
                return Result<FloorballMatchDto>.Failure("An error occurred while starting the period.");
            }
        }
    }
}

