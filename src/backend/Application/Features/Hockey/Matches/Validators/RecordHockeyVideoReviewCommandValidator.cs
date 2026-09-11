using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyVideoReviewCommandValidator : AbstractValidator<RecordHockeyVideoReviewCommand>
{
    public RecordHockeyVideoReviewCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReviewType).IsInEnum();
        RuleFor(x => x.OriginalDecision).IsInEnum();
        RuleFor(x => x.FinalDecision).IsInEnum();
    }
}
