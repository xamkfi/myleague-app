using Application.Features.Auth.Commands;
using FluentValidation;

namespace Application.Features.Auth.Validators;

/// <summary>
/// Validator for the RevokeTokenCommand
/// </summary>
public class RevokeTokenValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
