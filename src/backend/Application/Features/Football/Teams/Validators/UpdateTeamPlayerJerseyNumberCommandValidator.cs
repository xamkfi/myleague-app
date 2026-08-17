using Application.Features.Football.Teams.Commands;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

/// <summary>
/// Validator for UpdateTeamPlayerJerseyNumberCommand
/// </summary>
public class UpdateTeamPlayerJerseyNumberCommandValidator : AbstractValidator<UpdateTeamPlayerJerseyNumberCommand>
{
    public UpdateTeamPlayerJerseyNumberCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");

        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");

        RuleFor(x => x.JerseyNumber)
            .InclusiveBetween(1, 99)
            .When(x => x.JerseyNumber.HasValue)
            .WithMessage("Jersey number must be between 1 and 99");
    }
}
