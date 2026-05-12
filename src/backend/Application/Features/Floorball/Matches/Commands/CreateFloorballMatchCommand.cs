using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands
{
    /// <summary>
    /// Command for creating a floorball match
    /// </summary>
    /// <param name="CompetitionId"></param>
    /// <param name="HomeTeamId"></param>
    /// <param name="AwayTeamId"></param>
    /// <param name="RefereeId"></param>
    /// <param name="ScheduledDateTime"></param>
    /// <param name="Venue"></param>
    /// <param name="TournamentGroupId">Optional group ID for tournament group-stage matches</param>
    /// <param name="TournamentStage">Optional stage for tournament matches</param>
    public record CreateFloorballMatchCommand(
        Guid? CompetitionId,
        Guid? HomeTeamId,
        Guid? AwayTeamId,
        Guid? RefereeId,
        DateTime ScheduledDateTime,
        string? Venue,
        Guid? TournamentGroupId = null,
        FloorballTournamentStage? TournamentStage = null) : IRequest<Result<FloorballMatchDto>>;
}
