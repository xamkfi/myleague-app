using Application.Features.Common.Users.Commands;
using FluentValidation;

namespace Application.Features.Common.Users.Validators;

/// <summary>
/// Validator for DeleteUserCommand
/// </summary>
public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty().WithMessage("Requesting user ID is required")
            .NotEqual(Guid.Empty).WithMessage("Requesting user ID cannot be empty");
    }
}
