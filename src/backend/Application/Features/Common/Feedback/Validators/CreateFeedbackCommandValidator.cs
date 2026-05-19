// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Feedback.Commands;
using FluentValidation;

namespace Application.Features.Common.Feedback.Validators
{
    /// <summary>
    /// Validator for CreateFeedbackCommand
    /// </summary>
    public class CreateFeedbackCommandValidator : AbstractValidator<CreateFeedbackCommand>
    {
        public CreateFeedbackCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty")
                .MaximumLength(255).WithMessage("Title cannot be longer than 255 characters");

            RuleFor(x => x.FeedbackBody)
                .NotEmpty().WithMessage("Feedback needs to have content");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("The email address needs to be valid")
                .MaximumLength(255).WithMessage("Email cannot be longer than 255 characters")
                .When(x => !string.IsNullOrEmpty(x.Email)); 
        }
    }
}
