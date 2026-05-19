using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command to add a team into a specific division of a season
    /// </summary>
    public record AddTeamToSeasonDivisionCommand(
        Guid CompetitionId,
        Guid DivisionId,
        Guid TeamId) : IRequest<Result>;
}


