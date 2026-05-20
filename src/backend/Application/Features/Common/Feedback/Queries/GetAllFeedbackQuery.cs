// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.Feedback.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Common.Feedback.Queries
{
    public record GetAllFeedbackQuery(
        int page = 1,
        int pageSize = 0) : IRequest<Result<PagedResult<FeedbackListDto>>>
    {
        /// <summary>
        /// Resource for pagination configuration
        /// </summary>
        public const string ResourceKey = "Feedback";
    }
}
