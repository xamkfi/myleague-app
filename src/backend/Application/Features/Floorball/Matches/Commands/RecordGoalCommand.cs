using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands
{
    /// <summary>
    /// Command for recording a goal in a floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="ScoringTeamId"></param>
    /// <param name="ScoringPlayerId"></param>
    /// <param name="AssistingPlayerId"></param>
    /// <param name="SecondaryAssistingPlayerId"></param>
    /// <param name="PeriodNumber"></param>
    /// <param name="TimeInSeconds"></param>
    /// <param name="Description"></param>
    /// <param name="GoalType"></param>
    public record RecordGoalCommand(
        Guid MatchId,
        Guid ScoringTeamId,
        Guid ScoringPlayerId,
        Guid? AssistingPlayerId,
        Guid? SecondaryAssistingPlayerId,
        int PeriodNumber,
        int TimeInSeconds,
        string? Description,
        int? GoalType = null) : IRequest<Result<FloorballMatchDto>>;
}
