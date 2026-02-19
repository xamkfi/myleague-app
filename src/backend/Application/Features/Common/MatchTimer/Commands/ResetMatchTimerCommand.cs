using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.MatchTimer.Commands
{
    /// <summary>
    /// Command for resetting the timer of a match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    public record ResetMatchTimerCommand(Guid MatchId) : IRequest<Result>;
} 
