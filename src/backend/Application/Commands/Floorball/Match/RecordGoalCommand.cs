using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for recording a goal in a floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="ScoringTeamId"></param>
    /// <param name="ScoringPlayerId"></param>
    /// <param name="AssistingPlayerId"></param>
    /// <param name="PeriodNumber"></param>
    /// <param name="TimeInSeconds"></param>
    /// <param name="Description"></param>
    /// <param name="GoalType"></param>
    public record RecordGoalCommand(
        Guid MatchId,
        Guid ScoringTeamId,
        Guid ScoringPlayerId,
        Guid? AssistingPlayerId,
        int PeriodNumber,
        int TimeInSeconds,
        string? Description,
        int? GoalType = null) : IRequest<Result<FloorballMatchDto>>;
}
