using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a failed coach-challenge penalty linked to an existing video review.
/// </summary>
public record RecordHockeyFailedCoachChallengePenaltyCommand(
    Guid MatchId,
    Guid VideoReviewId,
    Guid PenaltyMatchTeamId,
    bool Enabled,
    int MaxChallengesPerTeam,
    bool LoseChallengeAfterFailed,
    bool PenaltyForFailedChallenge,
    int FailedChallengePenaltyMinutes,
    HockeyPenaltyOffence FailedChallengePenaltyOffence,
    HockeyPenaltySeverity FailedChallengePenaltySeverity,
    bool AllowChallengeInOvertime,
    bool AllowChallengeInShootout) : IRequest<Result<HockeyMatchDto>>;
