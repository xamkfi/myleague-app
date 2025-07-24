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
    /// Handler for starting the timer of a floorball match
    /// </summary>
    public class StartMatchTimerHandler : IRequestHandler<StartMatchTimerCommand, Result>
    {
        private readonly IMatchClockManager _clockManager;
        private readonly ILogger<StartMatchTimerHandler> _logger;

        public StartMatchTimerHandler(IMatchClockManager clockManager, ILogger<StartMatchTimerHandler> logger)
        {
            _clockManager = clockManager;
            _logger = logger;
        }

        public Task<Result> Handle(StartMatchTimerCommand request, CancellationToken cancellationToken)
        {
            if (!_clockManager.Exists(request.MatchId))
            {
                _logger.LogWarning("Timer does not exist for match {MatchId}, creating new timer.", request.MatchId);
            }
            _clockManager.Start(request.MatchId);
            _logger.LogInformation("Started timer for match {MatchId}", request.MatchId);
            return Task.FromResult(Result.Success());
        }
    }
} 