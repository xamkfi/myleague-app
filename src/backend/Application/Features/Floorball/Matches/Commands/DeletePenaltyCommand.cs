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
    /// Command for deleting a penalty event from a non-event-sourced match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="PenaltyEventId"></param>
    public record DeletePenaltyCommand(
        Guid MatchId,
        Guid PenaltyEventId) : IRequest<Result<FloorballMatchDto>>;
}


