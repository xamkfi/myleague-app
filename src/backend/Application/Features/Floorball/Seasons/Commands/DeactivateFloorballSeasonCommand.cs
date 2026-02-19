using System;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using MediatR;

namespace Application.Features.Floorball.Seasons.Commands
{
    /// <summary>
    /// Command for deactivating a floorball season
    /// </summary>
    /// <param name="Id">The ID of the season to deactivate</param>
    public record DeactivateFloorballSeasonCommand(Guid Id) : IRequest<Result<FloorballSeasonDto>>;
} 