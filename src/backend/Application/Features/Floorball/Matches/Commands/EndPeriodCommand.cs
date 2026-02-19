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

namespace Application.Features.Floorball.Matches.Commands
{
    /// <summary>
    /// Command for ending a period in a floorball match
    /// </summary>
    /// <param name="MatchId">The match identifier</param>
    /// <param name="PeriodNumber">The period number to end</param>
    public record EndPeriodCommand(
        Guid MatchId,
        int PeriodNumber) : IRequest<Result<FloorballMatchDto>>;
}


