using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyFailedCoachChallengePenaltyCommandValidator
    : AbstractValidator<RecordHockeyFailedCoachChallengePenaltyCommand>
{
    public RecordHockeyFailedCoachChallengePenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.VideoReviewId).NotEmpty();
        RuleFor(x => x.PenaltyMatchTeamId).NotEmpty();
        RuleFor(x => x.MaxChallengesPerTeam).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FailedChallengePenaltyMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FailedChallengePenaltyOffence).IsInEnum();
        RuleFor(x => x.FailedChallengePenaltySeverity).IsInEnum();
    }
}
