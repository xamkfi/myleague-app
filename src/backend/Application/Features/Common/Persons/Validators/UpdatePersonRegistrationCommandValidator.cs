using System;
using Application.Commands.Persons;
using FluentValidation;

namespace Application.Validators.Commands.Person
{
    /// <summary>
    /// Validator for UpdatePersonRegistrationCommand
    /// </summary>
    public class UpdatePersonRegistrationCommandValidator : AbstractValidator<UpdatePersonRegistrationCommand>
    {
        public UpdatePersonRegistrationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");
        }
    }
} 