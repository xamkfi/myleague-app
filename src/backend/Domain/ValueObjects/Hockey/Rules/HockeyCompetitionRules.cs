using Domain.Enums.Hockey.Competitions;

namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Aggregated rule configuration for a hockey competition.
/// </summary>
public class HockeyCompetitionRules : IEquatable<HockeyCompetitionRules>
{
    public string Name { get; private set; } = string.Empty;
    public string? RuleBookVersion { get; private set; }
    public HockeyRuleBookSource RuleBookSource { get; private set; }
    public HockeyMatchRules MatchRules { get; private set; } = null!;
    public HockeyStandingRules StandingRules { get; private set; } = null!;
    public HockeyRosterRules RosterRules { get; private set; } = null!;
    public HockeyVideoReviewRules? VideoReviewRules { get; private set; }
    public HockeyContactRules? ContactRules { get; private set; }

    private HockeyCompetitionRules() { }

    public HockeyCompetitionRules(
        string name,
        string? ruleBookVersion,
        HockeyRuleBookSource ruleBookSource,
        HockeyMatchRules matchRules,
        HockeyStandingRules standingRules,
        HockeyRosterRules rosterRules,
        HockeyVideoReviewRules? videoReviewRules = null,
        HockeyContactRules? contactRules = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rules name cannot be null or empty.", nameof(name));
        ArgumentNullException.ThrowIfNull(matchRules);
        ArgumentNullException.ThrowIfNull(standingRules);
        ArgumentNullException.ThrowIfNull(rosterRules);

        Name = name;
        RuleBookVersion = ruleBookVersion;
        RuleBookSource = ruleBookSource;
        MatchRules = matchRules;
        StandingRules = standingRules;
        RosterRules = rosterRules;
        VideoReviewRules = videoReviewRules;
        ContactRules = contactRules;
    }

    public static HockeyCompetitionRules Default() =>
        new("Default", null, HockeyRuleBookSource.LeagueSpecific,
            HockeyMatchRules.Default(), HockeyStandingRules.Default(), HockeyRosterRules.Default(),
            HockeyVideoReviewRules.Disabled(), HockeyContactRules.Default());

    public HockeyCompetitionRules WithMatchRules(HockeyMatchRules matchRules)
    {
        ArgumentNullException.ThrowIfNull(matchRules);
        return new HockeyCompetitionRules(Name, RuleBookVersion, RuleBookSource, matchRules,
            StandingRules, RosterRules, VideoReviewRules, ContactRules);
    }

    public override bool Equals(object? obj) => Equals(obj as HockeyCompetitionRules);

    public bool Equals(HockeyCompetitionRules? other)
    {
        if (other is null) return false;
        return Name == other.Name
            && RuleBookVersion == other.RuleBookVersion
            && RuleBookSource == other.RuleBookSource
            && Equals(MatchRules, other.MatchRules)
            && Equals(StandingRules, other.StandingRules)
            && Equals(RosterRules, other.RosterRules)
            && Equals(VideoReviewRules, other.VideoReviewRules)
            && Equals(ContactRules, other.ContactRules);
    }

    public override int GetHashCode() =>
        HashCode.Combine(Name, RuleBookVersion, RuleBookSource, MatchRules, StandingRules, RosterRules,
            VideoReviewRules, ContactRules);

    public static bool operator ==(HockeyCompetitionRules? left, HockeyCompetitionRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyCompetitionRules? left, HockeyCompetitionRules? right) => !(left == right);
}
