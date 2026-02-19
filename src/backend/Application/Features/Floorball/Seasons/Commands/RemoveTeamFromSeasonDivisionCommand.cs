using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command to remove a team from a specific division of a season
    /// </summary>
    public record RemoveTeamFromSeasonDivisionCommand(
        Guid SeasonId,
        Guid DivisionId,
        Guid TeamId) : IRequest<Result>;
}


