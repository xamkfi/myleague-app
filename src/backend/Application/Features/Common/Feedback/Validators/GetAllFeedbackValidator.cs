// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Feedback.Queries;
using Application.Services.Common;
using FluentValidation;

namespace Application.Features.Common.Feedback.Validators
{
    /// <summary>
    /// Validator for GetAllFeedbackQuery
    /// </summary>
    public class GetAllFeedbackQueryValidator : AbstractValidator<GetAllFeedbackQuery>
    {
        private readonly IPaginationService _paginationService;
        public GetAllFeedbackQueryValidator(IPaginationService paginationService)
        {
            _paginationService = paginationService;

            RuleFor(x => x.page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(x => x.pageSize)
                .Must(BeValidPageSize).WithMessage(GetPaginationErrorMessage());

        }

        private bool BeValidPageSize(int pageSize)
        {
            return _paginationService.IsValidPageSize(GetAllFeedbackQuery.ResourceKey, pageSize);
        }

        private string GetPaginationErrorMessage()
        {
            PaginationSettings settings = _paginationService.GetPaginationSettings(GetAllFeedbackQuery.ResourceKey);
            return $"Page size must be 0 (Use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
        }
    }
}
