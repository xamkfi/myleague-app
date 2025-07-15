using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Team
{
    /// <summary>
    /// Command for updating a floorball team's logo
    /// </summary>
    /// <param name="Id">The ID of the team to update</param>
    /// <param name="LogoUrl">The new logo URL (optional)</param>
    public record UpdateFloorballTeamLogoCommand(
        Guid Id,
        string? LogoUrl) : IRequest<Result<FloorballTeamDto>>;
} 