using Application.Features.Auth.Commands;
using FluentValidation;

namespace Application.Features.Auth.Validators;

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
