using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

public class CreateHockeyTeamCommandValidator : AbstractValidator<CreateHockeyTeamCommand>
{
    public CreateHockeyTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(200).WithMessage("Team name cannot exceed 200 characters.");

        RuleFor(x => x.ClubId).NotEmpty().WithMessage("Club id is required.");

        RuleFor(x => x.TeamCategory).IsInEnum().WithMessage("Team category is invalid.");

        RuleFor(x => x.ShortName)
            .MaximumLength(50).WithMessage("Short name cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ShortName));
    }
}
