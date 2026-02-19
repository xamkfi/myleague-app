using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match;

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
