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
    /// Command for recording one or more saves in a floorball match. <see cref="Count"/>
    /// defaults to 1 to preserve single-save call sites; values greater than 1 are used by
    /// the bulk backfill flow (recorder missed marking individual saves during play) and
    /// produce <c>Count</c> distinct save events at the same (period, time) coordinate
    /// inside a single transaction.
    /// </summary>
    public record RecordSaveCommand(
        Guid MatchId,
        Guid TeamId,
        Guid GoalieId,
        int PeriodNumber,
        int TimeInSeconds,
        bool WasInOvertime,
        bool WasInShootout,
        int Count = 1) : IRequest<Result<FloorballMatchDto>>;
}


