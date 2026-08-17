using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class ConfirmHockeyMatchRosterCommandValidator : AbstractValidator<ConfirmHockeyMatchRosterCommand>
{
    public ConfirmHockeyMatchRosterCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.TeamPlayerIds).NotEmpty().WithMessage("At least one team player is required.");
        RuleFor(x => x.Source).IsInEnum();
    }
}
