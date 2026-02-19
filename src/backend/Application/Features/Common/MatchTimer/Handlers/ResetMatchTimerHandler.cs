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
    /// Handler for resetting the timer of a match
    /// </summary>
    public class ResetMatchTimerHandler : IRequestHandler<ResetMatchTimerCommand, Result>
    {
        private readonly IMatchTimerService _timerService;
        private readonly ILogger<ResetMatchTimerHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the ResetMatchTimerHandler class
        /// </summary>
        /// <param name="timerService">The timer service</param>
        /// <param name="logger">The logger</param>
        public ResetMatchTimerHandler(IMatchTimerService timerService, ILogger<ResetMatchTimerHandler> logger)
        {
            _timerService = timerService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the reset timer command
        /// </summary>
        /// <param name="request">The reset timer command</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of the operation</returns>
        public async Task<Result> Handle(ResetMatchTimerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                bool exists = await _timerService.ExistsAsync(request.MatchId);
                if (!exists)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}", request.MatchId);
                    return Result.Failure($"Timer does not exist for match {request.MatchId}");
                }
                
                await _timerService.ResetTimerAsync(request.MatchId);
                _logger.LogInformation("Reset timer for match {MatchId}", request.MatchId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting timer for match {MatchId}", request.MatchId);
                return Result.Failure($"Failed to reset timer for match {request.MatchId}");
            }
        }
    }
} 