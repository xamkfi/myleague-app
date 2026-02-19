using System;
using Application.Commands.Persons;
using FluentValidation;

namespace Application.Validators.Commands.Person
{
    /// <summary>
    /// Validator for UpdatePersonRoleCommand
    /// </summary>
    public class UpdatePersonRoleCommandValidator : AbstractValidator<UpdatePersonRoleCommand>
    {
        public UpdatePersonRoleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Role must be a valid PersonRole value");
        }
    }
} 