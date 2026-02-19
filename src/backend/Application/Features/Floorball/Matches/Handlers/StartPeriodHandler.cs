using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Floorball.Matches.Commands;
using Application.Common;
using Application.Features.Common.MatchTimer.Services;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers
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

                // Validate period number (max = shootout period number, which is numberOfPeriods + 2)
                int maxPeriod = match.ShootoutPeriodNumber;
                if (request.PeriodNumber < 1 || request.PeriodNumber > maxPeriod)
                {
                    _logger.LogWarning("Invalid period number {PeriodNumber} for match {MatchId}", request.PeriodNumber, request.MatchId);
                    return Result<FloorballMatchDto>.Failure($"Period number must be between 1 and {maxPeriod}. Received: {request.PeriodNumber}");
                }

                // Ensure period score exists for this period
                // Regular periods are created at match creation
                // OT and Shootout periods are created by RecordOvertime/RecordShootout
                if (!match.PeriodScores.Any(ps => ps.PeriodNumber == request.PeriodNumber))
                {
                    _logger.LogWarning("Period score not initialized for period {PeriodNumber} in match {MatchId}", request.PeriodNumber, request.MatchId);
                    return Result<FloorballMatchDto>.Failure($"Period {request.PeriodNumber} has not been initialized. For overtime/shootout, call the appropriate record endpoint first.");
                }

                _logger.LogInformation("Starting period {PeriodNumber} for match {MatchId}", request.PeriodNumber, request.MatchId);

                // Reset and start timer for new period (except shootout which has no timer)
                // The timer service will automatically reset if the period changed
                if (request.PeriodNumber != match.ShootoutPeriodNumber)
                {
                    await _timerService.StartTimerAsync(match.Id, request.PeriodNumber);
                }
                else
                {
                    _logger.LogInformation("Skipping timer start for shootout (period {Period}) in match {MatchId}", request.PeriodNumber, request.MatchId);
                }
                
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

