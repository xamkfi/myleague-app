using System;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.Teams.Commands
{
    /// <summary>
    /// Command for updating a football team's logo
    /// </summary>
    /// <param name="Id">The ID of the team to update</param>
    /// <param name="LogoUrl">The new logo URL (optional)</param>
    public record UpdateFootballTeamLogoCommand(
        Guid Id,
        string? LogoUrl) : IRequest<Result<FootballTeamDto>>;
} 
