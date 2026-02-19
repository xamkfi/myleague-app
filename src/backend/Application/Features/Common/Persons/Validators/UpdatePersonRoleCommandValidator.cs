using System;
using Application.Features.Common.Persons.Commands;
using FluentValidation;

namespace Application.Features.Common.Persons.Validators
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
