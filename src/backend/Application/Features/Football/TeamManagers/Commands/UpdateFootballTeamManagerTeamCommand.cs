using System;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.TeamManagers.Commands
{
    /// <summary>
    /// Command for updating the team assignment of a football team manager
    /// </summary>
    /// <param name="Id">The ID of the team manager to update</param>
    /// <param name="TeamId">The new team ID this manager will be responsible for</param>
    public record UpdateFootballTeamManagerTeamCommand(
        Guid Id,
        Guid TeamId) : IRequest<Result<FootballTeamManagerDto>>;
} 
