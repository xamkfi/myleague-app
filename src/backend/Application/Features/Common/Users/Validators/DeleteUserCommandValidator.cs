using System;
using Application.Commands.Users;
using FluentValidation;

namespace Application.Validators.Commands.Users
{
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
        }
    }
} 