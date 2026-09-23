using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Competitions.Commands
{
    /// <summary>
    /// Command to add a division to a season
    /// </summary>
    public record AddFloorballDivisionToSeasonCommand(
        Guid CompetitionId,
        Guid DivisionId) : IRequest<Result>;
}


