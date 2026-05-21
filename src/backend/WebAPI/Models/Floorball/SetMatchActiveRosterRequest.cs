using Domain.Enums.Floorball;

namespace WebAPI.Models.Floorball;

/// <summary>
/// Request payload for replacing the active field player lineup (and optional goalie) of a single
/// team in a match. Each <see cref="ActiveRosterPlayer"/> entry carries the per-match role so
/// the UI can categorise lineups (defenders / forwards / centers).
/// </summary>
public class SetMatchActiveRosterRequest
{
    /// <summary>
    /// Per-match selections (player ID + role) that make up the active field lineup. Pass an
    /// empty list to clear the lineup. Must not include the goalie ID.
    /// </summary>
    public List<ActiveRosterPlayer> Players { get; set; } = new();

    /// <summary>
    /// Optional goalie player ID. When provided the active goalie is updated alongside the
    /// field players; when <c>null</c> the goalie remains unchanged.
    /// </summary>
    public Guid? GoalieId { get; set; }
}

/// <summary>
/// Single (player, role) pair in a <see cref="SetMatchActiveRosterRequest"/>.
/// </summary>
public class ActiveRosterPlayer
{
    /// <summary>Player ID being added to the lineup.</summary>
    public Guid PlayerId { get; set; }

    /// <summary>Per-match field role (Forward, Center or Defender).</summary>
    public FloorballPosition Position { get; set; }
}
