namespace Domain.ValueObjects.Football;

/// <summary>
/// Configurable rules specific to football tournaments (group vs knockout).
/// </summary>
public class FootballTournamentRules : IEquatable<FootballTournamentRules>
{
    public FootballMatchRules GroupStageMatchRules { get; private set; }
    public FootballMatchRules PlayoffMatchRules { get; private set; }
    public int TeamsAdvancingPerGroup { get; private set; }
    public bool HasPlayoffStage { get; private set; }
    public bool HasThirdPlaceMatch { get; private set; }

    private FootballTournamentRules()
    {
        GroupStageMatchRules = FootballMatchRules.Default();
        PlayoffMatchRules = FootballMatchRules.KnockoutDefault();
        TeamsAdvancingPerGroup = 2;
        HasPlayoffStage = true;
        HasThirdPlaceMatch = false;
    }

    public FootballTournamentRules(
        FootballMatchRules groupStageMatchRules,
        FootballMatchRules playoffMatchRules,
        int teamsAdvancingPerGroup,
        bool hasPlayoffStage,
        bool hasThirdPlaceMatch)
    {
        ArgumentNullException.ThrowIfNull(groupStageMatchRules);
        ArgumentNullException.ThrowIfNull(playoffMatchRules);

        if (hasPlayoffStage)
        {
            if (teamsAdvancingPerGroup < 1 || teamsAdvancingPerGroup > 8)
                throw new ArgumentOutOfRangeException(nameof(teamsAdvancingPerGroup), "Teams advancing per group must be between 1 and 8.");
        }

        GroupStageMatchRules = groupStageMatchRules;
        PlayoffMatchRules = playoffMatchRules;
        TeamsAdvancingPerGroup = hasPlayoffStage
            ? teamsAdvancingPerGroup
            : Math.Max(0, teamsAdvancingPerGroup);
        HasPlayoffStage = hasPlayoffStage;
        HasThirdPlaceMatch = hasPlayoffStage && hasThirdPlaceMatch;
    }

    public static FootballTournamentRules Default() =>
        new(
            FootballMatchRules.Default(),
            FootballMatchRules.KnockoutDefault(),
            teamsAdvancingPerGroup: 2,
            hasPlayoffStage: true,
            hasThirdPlaceMatch: false);

    public override bool Equals(object? obj) => Equals(obj as FootballTournamentRules);

    public bool Equals(FootballTournamentRules? other)
    {
        if (other is null)
            return false;
        return Equals(GroupStageMatchRules, other.GroupStageMatchRules)
            && Equals(PlayoffMatchRules, other.PlayoffMatchRules)
            && TeamsAdvancingPerGroup == other.TeamsAdvancingPerGroup
            && HasPlayoffStage == other.HasPlayoffStage
            && HasThirdPlaceMatch == other.HasThirdPlaceMatch;
    }

    public override int GetHashCode() =>
        HashCode.Combine(GroupStageMatchRules, PlayoffMatchRules, TeamsAdvancingPerGroup, HasPlayoffStage, HasThirdPlaceMatch);

    public static bool operator ==(FootballTournamentRules? left, FootballTournamentRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(FootballTournamentRules? left, FootballTournamentRules? right) => !(left == right);
}
