using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command to remove a division from a season
    /// </summary>
    public record RemoveDivisionFromSeasonCommand(
        Guid SeasonId,
        Guid DivisionId) : IRequest<Result>;
}


