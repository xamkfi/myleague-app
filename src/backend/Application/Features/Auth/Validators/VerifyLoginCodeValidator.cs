using Application.Commands.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

/// <summary>
/// Validator for the VerifyLoginCodeCommand
/// </summary>
public class VerifyLoginCodeValidator : AbstractValidator<VerifyLoginCodeCommand>
{
    public VerifyLoginCodeValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Login code is required.")
            .Length(6).WithMessage("Login code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Login code must contain only digits.");
    }
}
