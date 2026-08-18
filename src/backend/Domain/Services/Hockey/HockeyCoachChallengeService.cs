using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Hockey.Matches;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Services.Hockey;

/// <summary>
/// Result of handling a failed coach challenge.
/// </summary>
public sealed class HockeyCoachChallengeResult
{
    public bool ChallengeAllowed { get; init; }
    public int RemainingChallenges { get; init; }
    public bool LosesChallengeAfterFailure { get; init; }
    public HockeyPenalty? ResultingPenalty { get; init; }
    public HockeyDomainValidationResult Validation { get; init; } = HockeyDomainValidationResult.Ok();
}

/// <summary>
/// Orchestrates coach-challenge eligibility and failed-challenge penalties.
/// </summary>
public static class HockeyCoachChallengeService
{
    public static bool CanChallenge(
        HockeyMatch match,
        Guid challengingMatchTeamId,
        HockeyVideoReviewType reviewType,
        HockeyVideoReviewRules videoReviewRules)
    {
        return ValidateChallengeAllowed(match, challengingMatchTeamId, reviewType, videoReviewRules).IsValid;
    }

    public static HockeyDomainValidationResult ValidateChallengeAllowed(
        HockeyMatch match,
        Guid challengingMatchTeamId,
        HockeyVideoReviewType reviewType,
        HockeyVideoReviewRules videoReviewRules)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(videoReviewRules);
        List<string> errors = new();

        if (challengingMatchTeamId == Guid.Empty)
            errors.Add("Challenging match team id cannot be empty.");
        else if (!match.MatchTeams.Any(t => t.Id == challengingMatchTeamId))
            errors.Add("Challenging match team does not belong to this match.");

        if (!videoReviewRules.Enabled)
            errors.Add("Video review is not enabled.");
        if (!videoReviewRules.CoachChallengeAllowed)
            errors.Add("Coach challenges are not allowed.");

        HockeyCoachChallengeRules challengeRules = videoReviewRules.CoachChallengeRules;
        if (!challengeRules.Enabled)
            errors.Add("Coach challenge rules are disabled.");

        if (!IsReviewTypeAllowed(reviewType, videoReviewRules))
            errors.Add($"Review type {reviewType} cannot be coach-challenged under current rules.");

        if (match.WentToShootout && !challengeRules.AllowChallengeInShootout)
            errors.Add("Coach challenges are not allowed in the shootout.");
        else if (match.WentToOvertime && !match.WentToShootout && !challengeRules.AllowChallengeInOvertime)
            errors.Add("Coach challenges are not allowed in overtime.");

        if (errors.Count == 0 && challengingMatchTeamId != Guid.Empty)
        {
            int used = CountChallengesUsed(match, challengingMatchTeamId);
            if (used >= challengeRules.MaxChallengesPerTeam)
                errors.Add("No coach challenges remaining for this team.");
        }

        return errors.Count == 0
            ? HockeyDomainValidationResult.Ok()
            : HockeyDomainValidationResult.Fail(errors);
    }

    public static int GetRemainingChallenges(
        HockeyMatch match,
        Guid challengingMatchTeamId,
        HockeyCoachChallengeRules challengeRules)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(challengeRules);
        int used = CountChallengesUsed(match, challengingMatchTeamId);
        int remaining = challengeRules.MaxChallengesPerTeam - used;
        return remaining < 0 ? 0 : remaining;
    }

    public static HockeyCoachChallengeResult HandleFailedChallenge(
        HockeyMatch match,
        HockeyVideoReview review,
        HockeyCoachChallengeRules rules,
        Guid penaltyMatchTeamId)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(rules);

        List<string> errors = new();
        if (!review.IsCoachChallenge)
            errors.Add("Review is not a coach challenge.");
        if (review.WasSuccessful)
            errors.Add("Successful coach challenges do not produce a failed-challenge penalty.");
        if (!match.Events.Any(e => e.Id == review.Id))
            errors.Add("Video review must be recorded on the match before handling a failed challenge.");
        if (!match.MatchTeams.Any(t => t.Id == penaltyMatchTeamId))
            errors.Add("Penalty match team must belong to this match.");

        if (errors.Count > 0)
        {
            return new HockeyCoachChallengeResult
            {
                ChallengeAllowed = false,
                RemainingChallenges = GetRemainingChallenges(match, penaltyMatchTeamId, rules),
                LosesChallengeAfterFailure = rules.LoseChallengeAfterFailed,
                Validation = HockeyDomainValidationResult.Fail(errors)
            };
        }

        HockeyPenalty? penalty = null;
        if (rules.PenaltyForFailedChallenge)
            penalty = match.RecordFailedCoachChallengePenalty(review, rules, penaltyMatchTeamId);

        int remaining = GetRemainingChallenges(match, penaltyMatchTeamId, rules);
        if (rules.LoseChallengeAfterFailed && remaining > 0)
            remaining = 0;

        return new HockeyCoachChallengeResult
        {
            ChallengeAllowed = true,
            RemainingChallenges = remaining,
            LosesChallengeAfterFailure = rules.LoseChallengeAfterFailed,
            ResultingPenalty = penalty,
            Validation = HockeyDomainValidationResult.Ok()
        };
    }

    private static int CountChallengesUsed(HockeyMatch match, Guid matchTeamId) =>
        match.Events.OfType<HockeyVideoReview>()
            .Count(r => r.IsCoachChallenge && r.RequestedByMatchTeamId == matchTeamId);

    private static bool IsReviewTypeAllowed(HockeyVideoReviewType reviewType, HockeyVideoReviewRules rules) =>
        reviewType switch
        {
            HockeyVideoReviewType.PuckOverGoalLine => rules.ReviewPuckOverLine || rules.ReviewGoals,
            HockeyVideoReviewType.GoalBeforeTimeExpired => rules.ReviewGoals,
            HockeyVideoReviewType.HighStickGoal => rules.ReviewHighStickGoal || rules.ReviewGoals,
            HockeyVideoReviewType.GoalieInterference => rules.ReviewGoalieInterference,
            HockeyVideoReviewType.OffsideBeforeGoal => rules.ReviewOffsideBeforeGoal,
            HockeyVideoReviewType.KickingMotion => rules.ReviewGoals,
            HockeyVideoReviewType.PuckOutBeforeGoal => rules.ReviewGoals,
            HockeyVideoReviewType.PenaltyShotReview => rules.Enabled,
            _ => false
        };
}
