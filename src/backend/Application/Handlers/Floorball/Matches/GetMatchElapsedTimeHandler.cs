using Application.Queries.Floorball.Match;
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
    /// Handler for getting the elapsed time of a match timer
    /// </summary>
    public class GetMatchElapsedTimeHandler : IRequestHandler<GetMatchElapsedTimeQuery, Result<TimeSpan>>
    {
        private readonly IMatchClockManager _clockManager;
        private readonly ILogger<GetMatchElapsedTimeHandler> _logger;

        public GetMatchElapsedTimeHandler(IMatchClockManager clockManager, ILogger<GetMatchElapsedTimeHandler> logger)
        {
            _clockManager = clockManager;
            _logger = logger;
        }

        public Task<Result<TimeSpan>> Handle(GetMatchElapsedTimeQuery request, CancellationToken cancellationToken)
        {
            if (!_clockManager.Exists(request.MatchId))
            {
                _logger.LogWarning("Timer does not exist for match {MatchId}", request.MatchId);
                return Task.FromResult(Result<TimeSpan>.Failure($"Timer does not exist for match {request.MatchId}"));
            }
            var elapsed = _clockManager.GetElapsedTime(request.MatchId);
            _logger.LogInformation("Elapsed time for match {MatchId} is {Elapsed}", request.MatchId, elapsed);
            return Task.FromResult(Result<TimeSpan>.Success(elapsed));
        }
    }
} 