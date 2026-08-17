using Domain.Enums.Hockey.Matches;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyVideoReview : HockeyMatchEvent
{
    public HockeyVideoReviewType ReviewType { get; private set; }
    public HockeyReviewDecision OriginalDecision { get; private set; }
    public HockeyReviewDecision FinalDecision { get; private set; }

    public Guid? RequestedByMatchTeamId { get; private set; }
    public HockeyMatchTeam? RequestedByMatchTeam { get; private set; }

    public bool IsCoachChallenge { get; private set; }
    public bool WasSuccessful { get; private set; }

    public Guid? ResultingPenaltyId { get; private set; }
    public HockeyPenalty? ResultingPenalty { get; private set; }

    private HockeyVideoReview() { }

    public HockeyVideoReview(
        Guid matchId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyVideoReviewType reviewType,
        HockeyReviewDecision originalDecision,
        HockeyReviewDecision finalDecision,
        bool isCoachChallenge,
        bool wasSuccessful,
        Guid? requestedByMatchTeamId = null,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.VideoReview,
            periodNumber,
            gameTime,
            matchTeamId: requestedByMatchTeamId,
            description: description)
    {
        if (requestedByMatchTeamId == Guid.Empty)
            throw new ArgumentException("Requested-by match team id cannot be empty.", nameof(requestedByMatchTeamId));
        if (isCoachChallenge && requestedByMatchTeamId is null)
            throw new InvalidOperationException("Coach challenge requires a requesting match team.");

        ReviewType = reviewType;
        OriginalDecision = originalDecision;
        FinalDecision = finalDecision;
        RequestedByMatchTeamId = requestedByMatchTeamId;
        IsCoachChallenge = isCoachChallenge;
        WasSuccessful = wasSuccessful;
    }

    public void LinkResultingPenalty(HockeyPenalty penalty)
    {
        ArgumentNullException.ThrowIfNull(penalty);
        if (penalty.MatchId != MatchId)
            throw new InvalidOperationException("Resulting penalty must belong to the same match.");
        if (ResultingPenaltyId is not null)
            throw new InvalidOperationException("A resulting penalty is already linked to this review.");

        ResultingPenaltyId = penalty.Id;
        ResultingPenalty = penalty;
    }

    /// <summary>
    /// Creates a bench/delay-style penalty for a failed coach challenge when rules require it,
    /// and links it to this review.
    /// </summary>
    public HockeyPenalty CreateAndLinkFailedChallengePenalty(
        HockeyCoachChallengeRules rules,
        Guid penaltyMatchTeamId)
    {
        ArgumentNullException.ThrowIfNull(rules);
        if (!IsCoachChallenge)
            throw new InvalidOperationException("Only coach challenges can produce a failed-challenge penalty.");
        if (WasSuccessful)
            throw new InvalidOperationException("Successful coach challenges do not produce a penalty.");
        if (!rules.PenaltyForFailedChallenge)
            throw new InvalidOperationException("Coach challenge rules do not require a penalty for a failed challenge.");
        if (penaltyMatchTeamId == Guid.Empty)
            throw new ArgumentException("Penalty match team id cannot be empty.", nameof(penaltyMatchTeamId));
        if (RequestedByMatchTeamId is Guid requested && requested != penaltyMatchTeamId)
            throw new InvalidOperationException("Failed challenge penalty must be assessed to the challenging team.");

        bool isBenchPenalty = rules.FailedChallengePenaltySeverity == HockeyPenaltySeverity.BenchMinor;

        HockeyPenalty penalty = new(
            MatchId,
            penaltyMatchTeamId,
            PeriodNumber,
            GameTime,
            rules.FailedChallengePenaltySeverity,
            rules.FailedChallengePenaltyOffence,
            rules.FailedChallengePenaltyMinutes,
            isBenchPenalty: isBenchPenalty,
            description: "Failed coach challenge");

        LinkResultingPenalty(penalty);
        return penalty;
    }
}
