using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command to start a period in a floorball match
    /// </summary>
    public record StartPeriodCommand(Guid MatchId, int PeriodNumber) : IRequest<Result<FloorballMatchDto>>;
}

