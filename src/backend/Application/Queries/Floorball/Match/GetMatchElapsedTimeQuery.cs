using System;
using MediatR;
using Application.Common;

namespace Application.Queries.Floorball.Match
{
    /// <summary>
    /// Query for getting the elapsed time of a match timer
    /// </summary>
    /// <param name="MatchId"></param>
    public record GetMatchElapsedTimeQuery(Guid MatchId) : IRequest<Result<TimeSpan>>;
} 