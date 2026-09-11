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

namespace Application.Features.Football.Players.Commands
{
    /// <summary>
    /// Command for deleting a football player
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFootballPlayerCommand(Guid Id) : IRequest<Result>; 
}
