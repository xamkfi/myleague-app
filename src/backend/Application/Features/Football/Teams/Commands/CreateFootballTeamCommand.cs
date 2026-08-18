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
using Domain.Enums.Football;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Teams.Commands
{
    /// <summary>
    /// Command for creating a new football team
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="DivisionId"></param>
    /// <param name="ClubId"></param>
    /// <param name="HomeArena"></param>
    /// <param name="PrimaryJerseyColor"></param>
    /// <param name="TeamCategory"></param>
    /// <param name="SecondaryJerseyColor"></param>
    /// <param name="ShortName"></param>
    public record CreateFootballTeamCommand(
        string Name,
        Guid? DivisionId,
        Guid ClubId,
        string? HomeArena,
        string? PrimaryJerseyColor,
        TeamCategory? TeamCategory,
        string? SecondaryJerseyColor,
        string? ShortName) : IRequest<Result<FootballTeamDto>>;
}
