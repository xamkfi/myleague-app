using Application.Commands.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

/// <summary>
/// Validator for the RequestLoginCodeCommand
/// </summary>
public class RequestLoginCodeValidator : AbstractValidator<RequestLoginCodeCommand>
{
    public RequestLoginCodeValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
