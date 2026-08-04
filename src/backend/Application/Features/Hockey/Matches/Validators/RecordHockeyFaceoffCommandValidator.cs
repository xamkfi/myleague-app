using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyFaceoffCommandValidator : AbstractValidator<RecordHockeyFaceoffCommand>
{
    public RecordHockeyFaceoffCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.WinningMatchTeamId).NotEmpty();
        RuleFor(x => x.LosingMatchTeamId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Zone).IsInEnum();
        RuleFor(x => x.Spot).IsInEnum();
    }
}
