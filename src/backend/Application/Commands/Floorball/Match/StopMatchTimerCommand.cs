using System;
using MediatR;
using Application.Common;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for stopping the timer of a floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    public record StopMatchTimerCommand(Guid MatchId) : IRequest<Result>;
} 