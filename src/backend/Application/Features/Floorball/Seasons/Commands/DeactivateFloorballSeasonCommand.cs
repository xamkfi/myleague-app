using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command for deactivating a floorball season
    /// </summary>
    /// <param name="Id">The ID of the season to deactivate</param>
    public record DeactivateFloorballSeasonCommand(Guid Id) : IRequest<Result<FloorballSeasonDto>>;
} 