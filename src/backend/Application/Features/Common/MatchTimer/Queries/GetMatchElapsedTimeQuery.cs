using System;
using MediatR;
using Application.Common;

namespace Application.Queries.Common
{
    /// <summary>
    /// Query for getting the elapsed time of a match timer
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    public record GetMatchElapsedTimeQuery(Guid MatchId) : IRequest<Result<TimeSpan>>;
} 