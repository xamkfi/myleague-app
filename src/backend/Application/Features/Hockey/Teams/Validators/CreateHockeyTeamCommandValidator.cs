using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeyTeamCommand"/>.
/// </summary>
public class CreateHockeyTeamCommandValidator : AbstractValidator<CreateHockeyTeamCommand>
{
    public CreateHockeyTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(100).WithMessage("Team name cannot exceed 100 characters.");

        RuleFor(x => x.ClubId).NotEmpty().WithMessage("Club id is required.");

        RuleFor(x => x.TeamCategory).IsInEnum().WithMessage("Team category is invalid.");

        RuleFor(x => x.ShortName)
            .MaximumLength(4).WithMessage("Short name cannot exceed 4 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ShortName));
    }
}
