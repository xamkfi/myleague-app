using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// One side of a hockey match (home or away). The match aggregate stores sides only through
/// <see cref="HockeyMatchTeam"/> rows keyed by <see cref="TeamSlot"/> — not via separate
/// HomeTeamId / AwayTeamId columns on <see cref="HockeyMatch"/>.
/// </summary>
public class HockeyMatchTeam : BaseEntity
{
    /// <summary>Gets the parent match identifier.</summary>
    public Guid MatchId { get; private set; }

    /// <summary>Gets the parent match aggregate.</summary>
    public HockeyMatch Match { get; private set; } = null!;

    /// <summary>Gets the underlying hockey team identifier.</summary>
    public Guid TeamId { get; private set; }

    /// <summary>Gets the team navigation (cross-context; ignored in hockey EF config).</summary>
    public HockeyTeam? Team { get; private set; }

    /// <summary>
    /// Gets the competition-team surrogate when this match belongs to a competition.
    /// Null for standalone / friendly matches without a competition.
    /// </summary>
    public Guid? CompetitionTeamId { get; private set; }

    /// <summary>Gets the competition-team navigation when present.</summary>
    public HockeyCompetitionTeam? CompetitionTeam { get; private set; }

    /// <summary>Gets whether this side is home or away.</summary>
    public HockeyTeamSlot TeamSlot { get; private set; }

    /// <summary>Gets the total goals scored by this side in the match.</summary>
    public int Goals { get; private set; }

    /// <summary>Gets whether the goalie has been pulled for an extra attacker.</summary>
    public bool IsGoaliePulled { get; private set; }

    /// <summary>
    /// Gets the active goalie match-player id when known.
    /// Full <c>HockeyMatchActivePlayer</c> linkage is added in a later roster ticket.
    /// </summary>
    public Guid? ActiveGoalieMatchPlayerId { get; private set; }

    /// <summary>Gets whether this side tracks on-ice players for the match.</summary>
    public bool TracksOnIcePlayers { get; private set; }

    private HockeyMatchTeam() { }

    internal HockeyMatchTeam(
        Guid matchId,
        Guid teamId,
        HockeyTeamSlot teamSlot,
        Guid? competitionTeamId,
        bool tracksOnIcePlayers)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (teamSlot is not (HockeyTeamSlot.Home or HockeyTeamSlot.Away))
            throw new ArgumentException("Match team slot must be Home or Away.", nameof(teamSlot));

        MatchId = matchId;
        TeamId = teamId;
        TeamSlot = teamSlot;
        CompetitionTeamId = competitionTeamId;
        TracksOnIcePlayers = tracksOnIcePlayers;
        Goals = 0;
        IsGoaliePulled = false;
    }

    internal void SetGoals(int goals)
    {
        if (goals < 0)
            throw new ArgumentOutOfRangeException(nameof(goals), "Goals cannot be negative.");
        Goals = goals;
    }

    internal void SetGoaliePulled(bool isGoaliePulled) => IsGoaliePulled = isGoaliePulled;

    internal void SetActiveGoalieMatchPlayerId(Guid? activeGoalieMatchPlayerId)
    {
        if (activeGoalieMatchPlayerId == Guid.Empty)
            throw new ArgumentException("Active goalie match player id cannot be empty.", nameof(activeGoalieMatchPlayerId));
        ActiveGoalieMatchPlayerId = activeGoalieMatchPlayerId;
    }

    internal void SetTracksOnIcePlayers(bool tracksOnIcePlayers) => TracksOnIcePlayers = tracksOnIcePlayers;
}
