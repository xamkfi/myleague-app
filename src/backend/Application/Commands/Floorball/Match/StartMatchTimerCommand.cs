using System;
using MediatR;
using Application.Common;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for starting the timer of a floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    public record StartMatchTimerCommand(Guid MatchId) : IRequest<Result>;
} 