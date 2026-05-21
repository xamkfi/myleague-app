using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Command to replace the active field player lineup (and optionally the active goalie) for a
/// single team in a match. Used by the match-management UI when the operator edits the lineup
/// via the "Edit lineup" dialog. Each entry in <see cref="Players"/> carries the per-match role
/// (Forward, Center or Defender) so the UI can categorise lineups by position.
/// </summary>
public class SetMatchActiveRosterCommand : IRequest<Result<FloorballMatchDto>>
{
    /// <summary>
    /// Gets or sets the ID of the match whose lineup is being updated.
    /// </summary>
    public Guid MatchId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the team whose lineup is being updated. Must be one of the match's
    /// participating teams.
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Gets or sets the per-match selections (player ID + role) that make up the active field
    /// lineup. Pass an empty list to clear the lineup. Must not contain the goalie ID.
    /// </summary>
    public List<ActivePlayerInput> Players { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional goalie player ID. When provided, the active goalie for the team
    /// is updated alongside the field players in the same operation. When <c>null</c> the existing
    /// goalie is left untouched.
    /// </summary>
    public Guid? GoalieId { get; set; }
}

/// <summary>
/// Single player + per-match role pair on a <see cref="SetMatchActiveRosterCommand"/>.
/// </summary>
public class ActivePlayerInput
{
    /// <summary>Player ID being added to the lineup.</summary>
    public Guid PlayerId { get; set; }

    /// <summary>Per-match field role (Forward, Center or Defender).</summary>
    public FloorballPosition Position { get; set; }
}
