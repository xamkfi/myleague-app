using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command to remove a team from a specific division of a season
    /// </summary>
    public record RemoveTeamFromSeasonDivisionCommand(
        Guid SeasonId,
        Guid DivisionId,
        Guid TeamId) : IRequest<Result>;
}


