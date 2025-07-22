using System;
using Application.Commands.Users;
using FluentValidation;

namespace Application.Validators.Commands.Users
{
    /// <summary>
    /// Validator for CreateUserCommand
    /// </summary>
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters")
                .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$").WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit");

            RuleFor(x => x.PersonId)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");
        }
    }
} 