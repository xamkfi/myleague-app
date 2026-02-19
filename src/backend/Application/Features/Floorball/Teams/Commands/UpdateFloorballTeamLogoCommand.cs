using System;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Teams.Commands
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
