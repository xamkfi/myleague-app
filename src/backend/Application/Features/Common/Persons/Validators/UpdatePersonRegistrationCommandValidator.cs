using System;
using Application.Features.Common.Persons.Commands;
using FluentValidation;

namespace Application.Features.Common.Persons.Validators
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
