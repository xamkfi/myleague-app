using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for starting a period in a floorball match
    /// </summary>
    /// <param name="MatchId">The match identifier</param>
    /// <param name="PeriodNumber">The period number to start</param>
    public record StartPeriodCommand(
        Guid MatchId,
        int PeriodNumber) : IRequest<Result<FloorballMatchDto>>;
}


