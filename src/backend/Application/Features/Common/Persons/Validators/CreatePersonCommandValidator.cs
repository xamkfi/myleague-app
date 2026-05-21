using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Persons.Commands;
using FluentValidation;

namespace Application.Features.Common.Persons.Validators
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
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Birth date cannot be in the future")
                .When(x => x.BirthDate.HasValue);


            // Address validation (optional)
            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address!.Street1)
                    .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address!.Street1));

                RuleFor(x => x.Address!.City)
                    .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address!.City));

                RuleFor(x => x.Address!.PostalCode)
                    .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address!.PostalCode));

                RuleFor(x => x.Address!.Country)
                    .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address!.Country));

                RuleFor(x => x.Address!.Street2)
                    .MaximumLength(200).WithMessage("Street address 2 cannot exceed 200 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address!.Street2));
            });

            // Contact info validation (optional). Email itself is optional too — only the format
            // and length are enforced when a value is supplied. Lets tournament-imported players
            // without contact details pass through without manufacturing fake emails.
            When(x => x.ContactInfo != null, () =>
            {
                RuleFor(x => x.ContactInfo!.Email)
                    .EmailAddress().WithMessage("Invalid email format")
                    .MaximumLength(200).WithMessage("Email cannot exceed 200 characters")
                    .When(x => !string.IsNullOrEmpty(x.ContactInfo!.Email));

                RuleFor(x => x.ContactInfo!.Phone)
                    .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters")
                    .When(x => !string.IsNullOrEmpty(x.ContactInfo!.Phone));

                RuleFor(x => x.ContactInfo!.AlternativePhone)
                    .MaximumLength(50).WithMessage("Alternative phone number cannot exceed 50 characters")
                    .When(x => !string.IsNullOrEmpty(x.ContactInfo!.AlternativePhone));
            });
        }
    }
}
