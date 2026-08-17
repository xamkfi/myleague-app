using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.TeamManagers.Commands
{
    /// <summary>
    /// Command for updating a football team manager
    /// </summary>
    /// <param name="Id">The ID of the team manager to update</param>
    /// <param name="IsActive">Whether the team manager is active</param>
    public record UpdateFootballTeamManagerCommand(
        Guid Id,
        bool IsActive) : IRequest<Result<FootballTeamManagerDto>>;
} 
