using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeyTeamCommand"/>.
/// </summary>
public class UpdateHockeyTeamCommandValidator : AbstractValidator<UpdateHockeyTeamCommand>
{
    public UpdateHockeyTeamCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team id is required.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TeamCategory).IsInEnum();
        RuleFor(x => x.ShortName)
            .MaximumLength(4)
            .When(x => !string.IsNullOrWhiteSpace(x.ShortName));
    }
}
