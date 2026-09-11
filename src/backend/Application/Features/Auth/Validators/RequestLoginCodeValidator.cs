using Application.Features.Auth.Commands;
using FluentValidation;

namespace Application.Features.Auth.Validators;

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
