using Domain.Enums.Hockey.Matches;

namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Coach challenge configuration for video review.
/// </summary>
public class HockeyCoachChallengeRules : IEquatable<HockeyCoachChallengeRules>
{
    public bool Enabled { get; private set; }
    public int MaxChallengesPerTeam { get; private set; }
    public bool LoseChallengeAfterFailed { get; private set; }
    public bool PenaltyForFailedChallenge { get; private set; }
    public int FailedChallengePenaltyMinutes { get; private set; }
    public HockeyPenaltyOffence FailedChallengePenaltyOffence { get; private set; }
    public HockeyPenaltySeverity FailedChallengePenaltySeverity { get; private set; }
    public bool AllowChallengeInOvertime { get; private set; }
    public bool AllowChallengeInShootout { get; private set; }

    private HockeyCoachChallengeRules() { }

    public HockeyCoachChallengeRules(
        bool enabled,
        int maxChallengesPerTeam,
        bool loseChallengeAfterFailed,
        bool penaltyForFailedChallenge,
        int failedChallengePenaltyMinutes,
        HockeyPenaltyOffence failedChallengePenaltyOffence,
        HockeyPenaltySeverity failedChallengePenaltySeverity,
        bool allowChallengeInOvertime,
        bool allowChallengeInShootout)
    {
        if (maxChallengesPerTeam < 0)
            throw new ArgumentOutOfRangeException(nameof(maxChallengesPerTeam), "Max challenges per team cannot be negative.");
        if (failedChallengePenaltyMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(failedChallengePenaltyMinutes), "Penalty minutes cannot be negative.");
        if (enabled && maxChallengesPerTeam < 1)
            throw new ArgumentOutOfRangeException(nameof(maxChallengesPerTeam), "Enabled coach challenges require at least one challenge per team.");
        if (penaltyForFailedChallenge && failedChallengePenaltyMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(failedChallengePenaltyMinutes), "Failed challenge penalty requires at least one minute.");

        Enabled = enabled;
        MaxChallengesPerTeam = maxChallengesPerTeam;
        LoseChallengeAfterFailed = loseChallengeAfterFailed;
        PenaltyForFailedChallenge = penaltyForFailedChallenge;
        FailedChallengePenaltyMinutes = failedChallengePenaltyMinutes;
        FailedChallengePenaltyOffence = failedChallengePenaltyOffence;
        FailedChallengePenaltySeverity = failedChallengePenaltySeverity;
        AllowChallengeInOvertime = allowChallengeInOvertime;
        AllowChallengeInShootout = allowChallengeInShootout;
    }

    public static HockeyCoachChallengeRules Disabled() =>
        new(false, 0, false, false, 0, HockeyPenaltyOffence.UnsportsmanlikeConduct,
            HockeyPenaltySeverity.Minor, false, false);

    public override bool Equals(object? obj) => Equals(obj as HockeyCoachChallengeRules);

    public bool Equals(HockeyCoachChallengeRules? other)
    {
        if (other is null) return false;
        return Enabled == other.Enabled
            && MaxChallengesPerTeam == other.MaxChallengesPerTeam
            && LoseChallengeAfterFailed == other.LoseChallengeAfterFailed
            && PenaltyForFailedChallenge == other.PenaltyForFailedChallenge
            && FailedChallengePenaltyMinutes == other.FailedChallengePenaltyMinutes
            && FailedChallengePenaltyOffence == other.FailedChallengePenaltyOffence
            && FailedChallengePenaltySeverity == other.FailedChallengePenaltySeverity
            && AllowChallengeInOvertime == other.AllowChallengeInOvertime
            && AllowChallengeInShootout == other.AllowChallengeInShootout;
    }

    public override int GetHashCode() =>
        HashCode.Combine(Enabled, MaxChallengesPerTeam, LoseChallengeAfterFailed, PenaltyForFailedChallenge,
            FailedChallengePenaltyMinutes, FailedChallengePenaltyOffence, FailedChallengePenaltySeverity,
            AllowChallengeInOvertime);

    public static bool operator ==(HockeyCoachChallengeRules? left, HockeyCoachChallengeRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyCoachChallengeRules? left, HockeyCoachChallengeRules? right) => !(left == right);
}
