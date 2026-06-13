// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.FeedbackToggle.Commands;
using FluentValidation;

namespace Application.Features.Common.FeedbackToggle.Validators
{
    /// <summary>
    /// Validator for the SaveFeedbacktoggle command
    /// </summary>
    public class SaveFeedbackToggleValidator : AbstractValidator<SaveFeedbackToggleCommand>
    {
        public SaveFeedbackToggleValidator()
        {
            RuleFor(x => x.IsEnabled).NotNull().WithMessage("Toggle state cannot be null");
        }
    }
}
