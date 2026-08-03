using Application.Features.Hockey.Competitions.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

public class AddTeamToHockeyCompetitionCommandValidator : AbstractValidator<AddTeamToHockeyCompetitionCommand>
{
    public AddTeamToHockeyCompetitionCommandValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty().WithMessage("Competition id is required.");
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team id is required.");
        RuleFor(x => x.Seed)
            .GreaterThan(0).WithMessage("Seed must be greater than zero.")
            .When(x => x.Seed.HasValue);
    }
}
