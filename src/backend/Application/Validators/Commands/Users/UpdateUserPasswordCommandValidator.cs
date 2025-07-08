using System;
using Application.Commands.Users;
using FluentValidation;

namespace Application.Validators.Commands.Users
{
    /// <summary>
    /// Validator for UpdateUserPasswordCommand
    /// </summary>
    public class UpdateUserPasswordCommandValidator : AbstractValidator<UpdateUserPasswordCommand>
    {
        public UpdateUserPasswordCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User ID is required")
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
                .MaximumLength(100).WithMessage("New password cannot exceed 100 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$").WithMessage("New password must contain at least one lowercase letter, one uppercase letter, and one digit");

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password");
        }
    }
} 