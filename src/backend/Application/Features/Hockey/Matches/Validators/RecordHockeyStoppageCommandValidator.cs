using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyStoppageCommandValidator : AbstractValidator<RecordHockeyStoppageCommand>
{
    public RecordHockeyStoppageCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.NextFaceoffZone!).IsInEnum().When(x => x.NextFaceoffZone.HasValue);
        RuleFor(x => x.NextFaceoffSpot!).IsInEnum().When(x => x.NextFaceoffSpot.HasValue);
    }
}
