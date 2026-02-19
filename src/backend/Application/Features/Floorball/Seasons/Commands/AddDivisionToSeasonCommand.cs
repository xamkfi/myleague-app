using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command to add a division to a season
    /// </summary>
    public record AddDivisionToSeasonCommand(
        Guid SeasonId,
        Guid DivisionId) : IRequest<Result>;
}


