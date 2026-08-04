using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class AddHomeAwayTeamsToHockeyMatchCommandValidator
    : AbstractValidator<AddHomeAwayTeamsToHockeyMatchCommand>
{
    public AddHomeAwayTeamsToHockeyMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.HomeTeamId).NotEmpty();
        RuleFor(x => x.AwayTeamId).NotEmpty()
            .NotEqual(x => x.HomeTeamId).WithMessage("Home and away teams must be different.");
    }
}
