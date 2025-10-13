using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command to add a division to a season
    /// </summary>
    public record AddDivisionToSeasonCommand(
        Guid SeasonId,
        Guid DivisionId) : IRequest<Result>;
}


