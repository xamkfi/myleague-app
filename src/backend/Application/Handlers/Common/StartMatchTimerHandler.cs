using Application.Commands.Common;
using Application.Services.Common;
using Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Common
{
    /// <summary>
    /// Handler for starting the timer of a match
    /// </summary>
    public class StartMatchTimerHandler : IRequestHandler<StartMatchTimerCommand, Result>
    {
        private readonly IMatchTimerService _timerService;
        private readonly ILogger<StartMatchTimerHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the StartMatchTimerHandler class
        /// </summary>
        /// <param name="timerService">The timer service</param>
        /// <param name="logger">The logger</param>
        public StartMatchTimerHandler(IMatchTimerService timerService, ILogger<StartMatchTimerHandler> logger)
        {
            _timerService = timerService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the start timer command
        /// </summary>
        /// <param name="request">The start timer command</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of the operation</returns>
        public async Task<Result> Handle(StartMatchTimerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                bool exists = await _timerService.ExistsAsync(request.MatchId);
                if (!exists)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}, creating new timer.", request.MatchId);
                    await _timerService.CreateTimerAsync(request.MatchId);
                }
                
                await _timerService.StartTimerAsync(request.MatchId, request.PeriodNumber);
                _logger.LogInformation("Started timer for match {MatchId} with period {PeriodNumber}", request.MatchId, request.PeriodNumber);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting timer for match {MatchId}", request.MatchId);
                return Result.Failure($"Failed to start timer for match {request.MatchId}");
            }
        }
    }
} 