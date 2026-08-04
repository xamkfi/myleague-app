using Application.Features.Hockey.Players.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Players.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeyPlayerCommand"/>.
/// </summary>
public class CreateHockeyPlayerCommandValidator : AbstractValidator<CreateHockeyPlayerCommand>
{
    public CreateHockeyPlayerCommandValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("Person id is required.");
        RuleFor(x => x.PrimaryPosition).IsInEnum();
        RuleFor(x => x.Shoots).IsInEnum();
        RuleFor(x => x.Catches!).IsInEnum().When(x => x.Catches.HasValue);
        RuleFor(x => x.LicenseNumber).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.LicenseNumber));
    }
}
