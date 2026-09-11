using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.MatchTimer.Commands
{
    /// <summary>
    /// Command for starting the timer of a match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    /// <param name="PeriodNumber">Optional period number</param>
    public record StartMatchTimerCommand(Guid MatchId, int? PeriodNumber = null) : IRequest<Result>;
} 
