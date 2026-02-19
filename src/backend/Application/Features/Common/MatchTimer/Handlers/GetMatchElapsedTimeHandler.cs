using Application.Features.Common.Divisions.Queries;
using Application.Features.Common.Search.Queries;
using Application.Features.Common.MatchTimer.Queries;
using Application.Services.Common;
using Application.Common;
using Application.Features.Common.MatchTimer.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Common.MatchTimer.Handlers
{
    /// <summary>
    /// Handler for getting the elapsed time of a match timer
    /// </summary>
    public class GetMatchElapsedTimeHandler : IRequestHandler<GetMatchElapsedTimeQuery, Result<TimeSpan>>
    {
        private readonly IMatchTimerService _timerService;
        private readonly ILogger<GetMatchElapsedTimeHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetMatchElapsedTimeHandler class
        /// </summary>
        /// <param name="timerService">The timer service</param>
        /// <param name="logger">The logger</param>
        public GetMatchElapsedTimeHandler(IMatchTimerService timerService, ILogger<GetMatchElapsedTimeHandler> logger)
        {
            _timerService = timerService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get elapsed time query
        /// </summary>
        /// <param name="request">The get elapsed time query</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result with elapsed time</returns>
        public async Task<Result<TimeSpan>> Handle(GetMatchElapsedTimeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                bool exists = await _timerService.ExistsAsync(request.MatchId);
                if (!exists)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}", request.MatchId);
                    return Result<TimeSpan>.Failure($"Timer does not exist for match {request.MatchId}");
                }
                
                TimeSpan elapsed = await _timerService.GetElapsedTimeAsync(request.MatchId);
                _logger.LogInformation("Elapsed time for match {MatchId} is {Elapsed}", request.MatchId, elapsed);
                return Result<TimeSpan>.Success(elapsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting elapsed time for match {MatchId}", request.MatchId);
                return Result<TimeSpan>.Failure($"Failed to get elapsed time for match {request.MatchId}");
            }
        }
    }
} 
