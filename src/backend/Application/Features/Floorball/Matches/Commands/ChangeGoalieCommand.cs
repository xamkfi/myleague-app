using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Command to change the active goalie during a floorball match
/// </summary>
public class ChangeGoalieCommand : IRequest<Result<FloorballMatchDto>>
{
    /// <summary>
    /// Gets or sets the ID of the match
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the team whose goalie is being changed
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the new goalie
    /// </summary>
    public Guid GoalieId { get; set; }
}
