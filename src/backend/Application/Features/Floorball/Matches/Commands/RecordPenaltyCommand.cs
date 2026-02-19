using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for recording a penalty in a non-event-sourced floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="PenaltyType"></param>
    /// <param name="Minutes"></param>
    /// <param name="PeriodNumber"></param>
    /// <param name="TimeInSeconds"></param>
    /// <param name="Description"></param>
    public record RecordPenaltyCommand(
        Guid MatchId,
        Guid TeamId,
        Guid? PlayerId,
        FloorballPenaltyType PenaltyType,
        int Minutes,
        int PeriodNumber,
        int TimeInSeconds,
        string Description) : IRequest<Result<FloorballMatchDto>>;
}


