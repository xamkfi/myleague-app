using System;
using Application.Commands.Users;
using FluentValidation;

namespace Application.Validators.Commands.Users
{
    /// <summary>
    /// Validator for UpdateUserCommand
    /// </summary>
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User ID is required")
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters")
                .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

            // Password validation (optional)
            When(x => !string.IsNullOrEmpty(x.Password), () =>
            {
                RuleFor(x => x.Password)
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                    .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$").WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit");
            });
        }
    }
} 