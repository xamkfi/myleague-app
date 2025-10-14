using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command to add a team into a specific division of a season
    /// </summary>
    public record AddTeamToSeasonDivisionCommand(
        Guid SeasonId,
        Guid DivisionId,
        Guid TeamId) : IRequest<Result>;
}


