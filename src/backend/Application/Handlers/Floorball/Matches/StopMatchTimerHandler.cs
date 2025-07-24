using Application.Commands.Floorball.Match;
using Application.Services.Common;
using Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches
{
    /// <summary>
    /// Handler for stopping the timer of a floorball match
    /// </summary>
    public class StopMatchTimerHandler : IRequestHandler<StopMatchTimerCommand, Result>
    {
        private readonly IMatchClockManager _clockManager;
        private readonly ILogger<StopMatchTimerHandler> _logger;

        public StopMatchTimerHandler(IMatchClockManager clockManager, ILogger<StopMatchTimerHandler> logger)
        {
            _clockManager = clockManager;
            _logger = logger;
        }

        public Task<Result> Handle(StopMatchTimerCommand request, CancellationToken cancellationToken)
        {
            if (!_clockManager.Exists(request.MatchId))
            {
                _logger.LogWarning("Timer does not exist for match {MatchId}", request.MatchId);
                return Task.FromResult(Result.Failure($"Timer does not exist for match {request.MatchId}"));
            }
            _clockManager.Stop(request.MatchId);
            _logger.LogInformation("Stopped timer for match {MatchId}", request.MatchId);
            return Task.FromResult(Result.Success());
        }
    }
} 