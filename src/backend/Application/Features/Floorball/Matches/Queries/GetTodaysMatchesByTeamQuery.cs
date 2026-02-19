using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Floorball.Matches.Queries;

/// <summary>
/// Query to get today's matches by team
/// </summary>
public class GetTodaysMatchesByTeamQuery : IRequest<Result<IEnumerable<FloorballMatchDto>>>
{
    /// <summary>
    /// Team's id
    /// </summary>
    public Guid TeamId { get; set; }

    public GetTodaysMatchesByTeamQuery(Guid teamId)
    {
        TeamId = teamId;
    }
} 
