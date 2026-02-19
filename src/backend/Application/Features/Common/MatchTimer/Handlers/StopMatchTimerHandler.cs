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
    /// Handler for stopping the timer of a match
    /// </summary>
    public class StopMatchTimerHandler : IRequestHandler<StopMatchTimerCommand, Result>
    {
        private readonly IMatchTimerService _timerService;
        private readonly ILogger<StopMatchTimerHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the StopMatchTimerHandler class
        /// </summary>
        /// <param name="timerService">The timer service</param>
        /// <param name="logger">The logger</param>
        public StopMatchTimerHandler(IMatchTimerService timerService, ILogger<StopMatchTimerHandler> logger)
        {
            _timerService = timerService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the stop timer command
        /// </summary>
        /// <param name="request">The stop timer command</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of the operation</returns>
        public async Task<Result> Handle(StopMatchTimerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                bool exists = await _timerService.ExistsAsync(request.MatchId);
                if (!exists)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}", request.MatchId);
                    return Result.Failure($"Timer does not exist for match {request.MatchId}");
                }
                
                await _timerService.StopTimerAsync(request.MatchId);
                _logger.LogInformation("Stopped timer for match {MatchId}", request.MatchId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping timer for match {MatchId}", request.MatchId);
                return Result.Failure($"Failed to stop timer for match {request.MatchId}");
            }
        }
    }
} 