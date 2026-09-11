using Domain.Entities;
using Domain.Enums.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a player that is part of a team's active lineup for a specific match. Each entry
/// pins the per-match role (Forward, Center or Defender) so the same player can be deployed
/// differently across matches without mutating their player profile.
///
/// Goalkeepers are tracked separately on <see cref="FloorballMatch.HomeActiveGoalieId"/> and
/// <see cref="FloorballMatch.AwayActiveGoalieId"/>; this entity covers field players only.
/// </summary>
public class FloorballMatchActivePlayer : BaseEntity
{
    /// <summary>
    /// Gets the ID of the match this lineup entry belongs to.
    /// </summary>
    public Guid MatchId { get; private set; }

    /// <summary>
    /// Gets the ID of the team for which the player is active. Always equals either
    /// <see cref="FloorballMatch.HomeTeamId"/> or <see cref="FloorballMatch.AwayTeamId"/>.
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the ID of the player marked as an active field player for the team.
    /// </summary>
    public Guid PlayerId { get; private set; }

    /// <summary>
    /// Gets the field role assigned to the player for this match. Constrained to
    /// <see cref="FloorballPosition.Forward"/>, <see cref="FloorballPosition.Center"/> or
    /// <see cref="FloorballPosition.Defender"/> by the constructor.
    /// </summary>
    public FloorballPosition Position { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballMatchActivePlayer() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FloorballMatchActivePlayer"/> class.
    /// </summary>
    public FloorballMatchActivePlayer(Guid matchId, Guid teamId, Guid playerId, FloorballPosition position) : base()
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match ID cannot be empty.", nameof(matchId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
        if (position == FloorballPosition.None || position == FloorballPosition.Goalkeeper)
            throw new ArgumentException(
                "Active field player must use a field role (Forward, Center or Defender). Goalies are tracked on the match itself.",
                nameof(position));

        MatchId = matchId;
        TeamId = teamId;
        PlayerId = playerId;
        Position = position;
    }
}
