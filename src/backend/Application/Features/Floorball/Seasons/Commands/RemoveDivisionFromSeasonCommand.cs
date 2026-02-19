using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command to remove a division from a season
    /// </summary>
    public record RemoveDivisionFromSeasonCommand(
        Guid SeasonId,
        Guid DivisionId) : IRequest<Result>;
}


