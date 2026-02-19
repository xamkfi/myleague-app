using System;
using MediatR;
using Application.Common;

namespace Application.Commands.Common
{
    /// <summary>
    /// Command for stopping the timer of a match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    public record StopMatchTimerCommand(Guid MatchId) : IRequest<Result>;
} 