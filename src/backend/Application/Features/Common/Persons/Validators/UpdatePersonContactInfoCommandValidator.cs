// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// Validator for UpdatePersonContactInfoCommand
    /// </summary>
    public class UpdatePersonContactInfoCommandValidator : AbstractValidator<UpdatePersonContactInfoCommand>
    {
        public UpdatePersonContactInfoCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

            RuleFor(x => x.contactInfo)
                .NotNull().WithMessage("Contact information is required");

            When(x => x.contactInfo != null, () =>
            {
                RuleFor(x => x.contactInfo.Email)
                    .NotEmpty().WithMessage("Email is required")
                    .EmailAddress().WithMessage("Invalid email format")
                    .MaximumLength(200).WithMessage("Email cannot exceed 200 characters");

                RuleFor(x => x.contactInfo.Phone)
                    .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters")
                    .When(x => !string.IsNullOrEmpty(x.contactInfo.Phone));

                RuleFor(x => x.contactInfo.AlternativePhone)
                    .MaximumLength(50).WithMessage("Alternative phone number cannot exceed 50 characters")
                    .When(x => !string.IsNullOrEmpty(x.contactInfo.AlternativePhone));
            });
        }
    }
}
