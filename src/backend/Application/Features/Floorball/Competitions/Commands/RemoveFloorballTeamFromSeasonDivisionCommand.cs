using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Competitions.Commands
{
    /// <summary>
    /// Command to remove a team from a specific division of a season
    /// </summary>
    public record RemoveFloorballTeamFromSeasonDivisionCommand(
        Guid CompetitionId,
        Guid DivisionId,
        Guid TeamId) : IRequest<Result>;
}


