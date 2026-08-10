namespace Domain.ValueObjects.Hockey.Rules;

/// <summary>
/// Video review and coach challenge rules for a hockey competition.
/// </summary>
public class HockeyVideoReviewRules : IEquatable<HockeyVideoReviewRules>
{
    public bool Enabled { get; private set; }
    public bool CoachChallengeAllowed { get; private set; }
    public bool ReviewGoals { get; private set; }
    public bool ReviewOffsideBeforeGoal { get; private set; }
    public bool ReviewGoalieInterference { get; private set; }
    public bool ReviewHighStickGoal { get; private set; }
    public bool ReviewPuckOverLine { get; private set; }
    public HockeyCoachChallengeRules CoachChallengeRules { get; private set; } = null!;

    private HockeyVideoReviewRules()
    {
        CoachChallengeRules = HockeyCoachChallengeRules.Disabled();
    }

    public HockeyVideoReviewRules(
        bool enabled,
        bool coachChallengeAllowed,
        bool reviewGoals,
        bool reviewOffsideBeforeGoal,
        bool reviewGoalieInterference,
        bool reviewHighStickGoal,
        bool reviewPuckOverLine,
        HockeyCoachChallengeRules coachChallengeRules)
    {
        ArgumentNullException.ThrowIfNull(coachChallengeRules);
        if (coachChallengeAllowed && !coachChallengeRules.Enabled)
            throw new ArgumentException("Coach challenge rules must be enabled when coach challenges are allowed.", nameof(coachChallengeRules));

        Enabled = enabled;
        CoachChallengeAllowed = coachChallengeAllowed;
        ReviewGoals = reviewGoals;
        ReviewOffsideBeforeGoal = reviewOffsideBeforeGoal;
        ReviewGoalieInterference = reviewGoalieInterference;
        ReviewHighStickGoal = reviewHighStickGoal;
        ReviewPuckOverLine = reviewPuckOverLine;
        CoachChallengeRules = coachChallengeRules;
    }

    public static HockeyVideoReviewRules Disabled() =>
        new(false, false, false, false, false, false, false, HockeyCoachChallengeRules.Disabled());

    public override bool Equals(object? obj) => Equals(obj as HockeyVideoReviewRules);

    public bool Equals(HockeyVideoReviewRules? other)
    {
        if (other is null) return false;
        return Enabled == other.Enabled
            && CoachChallengeAllowed == other.CoachChallengeAllowed
            && ReviewGoals == other.ReviewGoals
            && ReviewOffsideBeforeGoal == other.ReviewOffsideBeforeGoal
            && ReviewGoalieInterference == other.ReviewGoalieInterference
            && ReviewHighStickGoal == other.ReviewHighStickGoal
            && ReviewPuckOverLine == other.ReviewPuckOverLine
            && Equals(CoachChallengeRules, other.CoachChallengeRules);
    }

    public override int GetHashCode() =>
        HashCode.Combine(Enabled, CoachChallengeAllowed, ReviewGoals, ReviewOffsideBeforeGoal,
            ReviewGoalieInterference, ReviewHighStickGoal, ReviewPuckOverLine, CoachChallengeRules);

    public static bool operator ==(HockeyVideoReviewRules? left, HockeyVideoReviewRules? right) =>
        ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

    public static bool operator !=(HockeyVideoReviewRules? left, HockeyVideoReviewRules? right) => !(left == right);
}
