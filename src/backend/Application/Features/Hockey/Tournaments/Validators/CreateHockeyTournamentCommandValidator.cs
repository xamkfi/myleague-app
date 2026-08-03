using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

public class CreateHockeyTournamentCommandValidator : AbstractValidator<CreateHockeyTournamentCommand>
{
    public CreateHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tournament name is required.")
            .MaximumLength(200).WithMessage("Tournament name cannot exceed 200 characters.");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateTime)).WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateTime)).WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.Venue)
            .MaximumLength(200).WithMessage("Venue cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Venue));
    }
}
