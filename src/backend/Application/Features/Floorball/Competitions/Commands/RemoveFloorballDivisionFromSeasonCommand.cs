using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Competitions.Commands
{
    /// <summary>
    /// Command to remove a division from a season
    /// </summary>
    public record RemoveFloorballDivisionFromSeasonCommand(
        Guid CompetitionId,
        Guid DivisionId) : IRequest<Result>;
}


