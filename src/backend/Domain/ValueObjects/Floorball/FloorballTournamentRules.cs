namespace Domain.ValueObjects.Floorball;

/// <summary>
/// Configurable rules specific to tournament competitions.
/// Determines group-stage vs playoff rules, advancement, and bracket structure.
/// </summary>
public class FloorballTournamentRules : IEquatable<FloorballTournamentRules>
{
    /// <summary>
    /// Gets the match rules applied during the group stage.
    /// </summary>
    public FloorballMatchRules GroupStageMatchRules { get; private set; }

    /// <summary>
    /// Gets the match rules applied during the playoff stage.
    /// Playoffs may have different overtime/shootout rules than group stage.
    /// </summary>
    public FloorballMatchRules PlayoffMatchRules { get; private set; }

    /// <summary>
    /// Gets the number of teams that advance from each group to the playoff stage.
    /// </summary>
    public int TeamsAdvancingPerGroup { get; private set; }

    /// <summary>
    /// Gets whether the tournament includes a playoff stage after group stage.
    /// </summary>
    public bool HasPlayoffStage { get; private set; }

    /// <summary>
    /// Gets whether the tournament includes a third-place match.
    /// </summary>
    public bool HasThirdPlaceMatch { get; private set; }

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private FloorballTournamentRules()
    {
        GroupStageMatchRules = FloorballMatchRules.Default();
        PlayoffMatchRules = FloorballMatchRules.Default();
        TeamsAdvancingPerGroup = 2;
        HasPlayoffStage = true;
        HasThirdPlaceMatch = false;
    }

    public FloorballTournamentRules(
        FloorballMatchRules groupStageMatchRules,
        FloorballMatchRules playoffMatchRules,
        int teamsAdvancingPerGroup,
        bool hasPlayoffStage,
        bool hasThirdPlaceMatch)
    {
        ArgumentNullException.ThrowIfNull(groupStageMatchRules);
        ArgumentNullException.ThrowIfNull(playoffMatchRules);

        if (teamsAdvancingPerGroup < 1 || teamsAdvancingPerGroup > 8)
            throw new ArgumentOutOfRangeException(nameof(teamsAdvancingPerGroup), "Teams advancing per group must be between 1 and 8.");

        GroupStageMatchRules = groupStageMatchRules;
        PlayoffMatchRules = playoffMatchRules;
        TeamsAdvancingPerGroup = teamsAdvancingPerGroup;
        HasPlayoffStage = hasPlayoffStage;
        HasThirdPlaceMatch = hasThirdPlaceMatch;
    }

    public static FloorballTournamentRules Default()
    {
        return new FloorballTournamentRules(
            FloorballMatchRules.Default(),
            FloorballMatchRules.Default(),
            teamsAdvancingPerGroup: 2,
            hasPlayoffStage: true,
            hasThirdPlaceMatch: false);
    }

    public override bool Equals(object? obj) => Equals(obj as FloorballTournamentRules);

    public bool Equals(FloorballTournamentRules? other)
    {
        if (other is null) return false;
        return Equals(GroupStageMatchRules, other.GroupStageMatchRules)
            && Equals(PlayoffMatchRules, other.PlayoffMatchRules)
            && TeamsAdvancingPerGroup == other.TeamsAdvancingPerGroup
            && HasPlayoffStage == other.HasPlayoffStage
            && HasThirdPlaceMatch == other.HasThirdPlaceMatch;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GroupStageMatchRules, PlayoffMatchRules, TeamsAdvancingPerGroup, HasPlayoffStage, HasThirdPlaceMatch);
    }

    public static bool operator ==(FloorballTournamentRules? left, FloorballTournamentRules? right)
    {
        if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    public static bool operator !=(FloorballTournamentRules? left, FloorballTournamentRules? right) => !(left == right);
}
