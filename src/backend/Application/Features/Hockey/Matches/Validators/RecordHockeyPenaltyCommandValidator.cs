using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyPenaltyCommandValidator : AbstractValidator<RecordHockeyPenaltyCommand>
{
    public RecordHockeyPenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.PenaltyMatchTeamId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Offence).IsInEnum();
        RuleFor(x => x.PenaltyMinutes).GreaterThanOrEqualTo(0);
    }
}
