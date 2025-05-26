using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands.Persons;
using FluentValidation;

namespace Application.Validators.Commands.Person
{
    /// <summary>
    /// Validator for CreatePersonCommand
    /// </summary>
    public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
    {
        public CreatePersonCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Firstname is required")
                .MaximumLength(100).WithMessage("Firstname cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Lastname is required")
                .MaximumLength(100).WithMessage("Lastname cannot exceed 100 characters");

            RuleFor(x => x.BirthDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Birth date cannot be in the future");

            // Address validation (optional)
            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address.Street1)
                    .NotEmpty().WithMessage("Street address is required")
                    .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters");

                RuleFor(x => x.Address.City)
                    .NotEmpty().WithMessage("City is required")
                    .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

                RuleFor(x => x.Address.PostalCode)
                    .NotEmpty().WithMessage("Postal code is required")
                    .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

                RuleFor(x => x.Address.Country)
                    .NotEmpty().WithMessage("Country is required")
                    .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

                RuleFor(x => x.Address.Street2)
                    .MaximumLength(200).WithMessage("Street address 2 cannot exceed 200 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address.Street2));
            });

            // Contact info validation (optional)
            When(x => x.ContactInfo != null, () =>
            {
                RuleFor(x => x.ContactInfo.Email)
                    .NotEmpty().WithMessage("Email is required")
                    .EmailAddress().WithMessage("Invalid email format")
                    .MaximumLength(200).WithMessage("Email cannot exceed 200 characters");

                RuleFor(x => x.ContactInfo.Phone)
                    .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
                    .When(x => !string.IsNullOrEmpty(x.ContactInfo.Phone));

                RuleFor(x => x.ContactInfo.AlternativePhone)
                    .MaximumLength(20).WithMessage("Alternative phone number cannot exceed 20 characters")
                    .When(x => !string.IsNullOrEmpty(x.ContactInfo.AlternativePhone));
            });
        }
    }
}
