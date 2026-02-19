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
    /// Command for recording a save in a non-event-sourced floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="TeamId"></param>
    /// <param name="GoalieId"></param>
    /// <param name="PeriodNumber"></param>
    /// <param name="TimeInSeconds"></param>
    /// <param name="WasInOvertime"></param>
    /// <param name="WasInShootout"></param>
    public record RecordSaveCommand(
        Guid MatchId,
        Guid TeamId,
        Guid GoalieId,
        int PeriodNumber,
        int TimeInSeconds,
        bool WasInOvertime,
        bool WasInShootout) : IRequest<Result<FloorballMatchDto>>;
}


