using Domain.Enums.Hockey.Competitions;

namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Tournament-specific configuration rules.
/// </summary>
public class HockeyTournamentRules : IEquatable<HockeyTournamentRules>
{
    public HockeyTournamentFormat Format { get; private set; }
    public bool HasGroupStage { get; private set; }
    public bool HasPlayoffs { get; private set; }
    public bool HasBronzeGame { get; private set; }
    public bool HasPlacementGames { get; private set; }
    public int TeamsAdvancingPerGroup { get; private set; }
    public HockeyStandingRules? GroupStandingRules { get; private set; }
    public HockeyMatchRules? MatchRulesOverride { get; private set; }

    private HockeyTournamentRules() { }

    public HockeyTournamentRules(
        HockeyTournamentFormat format,
        bool hasGroupStage,
        bool hasPlayoffs,
        bool hasBronzeGame,
        bool hasPlacementGames,
        int teamsAdvancingPerGroup,
        HockeyStandingRules? groupStandingRules = null,
        HockeyMatchRules? matchRulesOverride = null)
    {
        if (hasPlayoffs && teamsAdvancingPerGroup < 1)
            throw new ArgumentOutOfRangeException(nameof(teamsAdvancingPerGroup), "Teams advancing per group must be at least 1 when playoffs are enabled.");

        Format = format;
        HasGroupStage = hasGroupStage;
        HasPlayoffs = hasPlayoffs;
        HasBronzeGame = hasPlayoffs && hasBronzeGame;
        HasPlacementGames = hasPlayoffs && hasPlacementGames;
        TeamsAdvancingPerGroup = hasPlayoffs ? teamsAdvancingPerGroup : 0;
        GroupStandingRules = groupStandingRules;
        MatchRulesOverride = matchRulesOverride;
    }

    public static HockeyTournamentRules Default() =>
        new(HockeyTournamentFormat.GroupsAndPlayoffs, hasGroupStage: true, hasPlayoffs: true,
            hasBronzeGame: false, hasPlacementGames: false, teamsAdvancingPerGroup: 2,
            HockeyStandingRules.Default());

    public override bool Equals(object? obj) => Equals(obj as HockeyTournamentRules);

    public bool Equals(HockeyTournamentRules? other)
    {
        if (other is null) return false;
        return Format == other.Format
            && HasGroupStage == other.HasGroupStage
            && HasPlayoffs == other.HasPlayoffs
            && HasBronzeGame == other.HasBronzeGame
            && HasPlacementGames == other.HasPlacementGames
            && TeamsAdvancingPerGroup == other.TeamsAdvancingPerGroup
            && Equals(GroupStandingRules, other.GroupStandingRules)
            && Equals(MatchRulesOverride, other.MatchRulesOverride);
    }

    public override int GetHashCode() =>
        HashCode.Combine(Format, HasGroupStage, HasPlayoffs, HasBronzeGame, HasPlacementGames,
            TeamsAdvancingPerGroup, GroupStandingRules, MatchRulesOverride);

    public static bool operator ==(HockeyTournamentRules? left, HockeyTournamentRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyTournamentRules? left, HockeyTournamentRules? right) => !(left == right);
}
