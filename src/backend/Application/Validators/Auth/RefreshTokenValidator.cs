using Application.Commands.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

/// <summary>
/// Validator for the RefreshTokenCommand
/// </summary>
public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
