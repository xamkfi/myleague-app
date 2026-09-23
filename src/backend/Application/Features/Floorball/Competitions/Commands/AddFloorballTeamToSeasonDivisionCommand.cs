using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Competitions.Commands
{
    /// <summary>
    /// Command to add a team into a specific division of a season
    /// </summary>
    public record AddFloorballTeamToSeasonDivisionCommand(
        Guid CompetitionId,
        Guid DivisionId,
        Guid TeamId,
        Domain.Enums.Common.RosterEnrollmentMode RosterMode = Domain.Enums.Common.RosterEnrollmentMode.CopyLatest) : IRequest<Result>;
}


