// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// Validator for UpdatePersonCommand
    /// </summary>
    public class UpdatePersonAddressCommandValidator : AbstractValidator<UpdatePersonAddressCommand>
    {
        public UpdatePersonAddressCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

            When(x => x.address != null, () =>
            {
                RuleFor(x => x.address.Street1)
                    .MaximumLength(200).WithMessage("Street address cannot exceed 200 characters")
                    .When(x => !string.IsNullOrEmpty(x.address.Street1));

                RuleFor(x => x.address.City)
                    .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
                    .When(x => !string.IsNullOrEmpty(x.address.City));

                RuleFor(x => x.address.PostalCode)
                    .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters")
                    .When(x => !string.IsNullOrEmpty(x.address.PostalCode));

                RuleFor(x => x.address.Country)
                    .NotEmpty().WithMessage("Country is required")
                    .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

                RuleFor(x => x.address.Street2)
                    .MaximumLength(200).WithMessage("Street address 2 cannot exceed 200 characters")
                    .When(x => !string.IsNullOrEmpty(x.address.Street2));
            });
        }
    }
}
