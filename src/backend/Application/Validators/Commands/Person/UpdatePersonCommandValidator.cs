using System;
using Application.Commands.Persons;
using FluentValidation;

namespace Application.Validators.Commands.Person
{
    /// <summary>
    /// Validator for UpdatePersonCommand
    /// </summary>
    public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
    {
        public UpdatePersonCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.BirthDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Birth date cannot be in the future");

            // Address validation (optional)
            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address.Street1)
                    .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address.Street1));

                RuleFor(x => x.Address.City)
                    .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address.City));

                RuleFor(x => x.Address.PostalCode)
                    .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
                    .When(x => !string.IsNullOrEmpty(x.Address.PostalCode));

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
                    .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters")
                    .When(x => !string.IsNullOrEmpty(x.ContactInfo.Phone));

                RuleFor(x => x.ContactInfo.AlternativePhone)
                    .MaximumLength(50).WithMessage("Alternative phone number cannot exceed 50 characters")
                    .When(x => !string.IsNullOrEmpty(x.ContactInfo.AlternativePhone));
            });
        }
    }
} 