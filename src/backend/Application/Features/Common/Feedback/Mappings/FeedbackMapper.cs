// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Feedback.DTOs;
using Domain.Entities.Common;

namespace Application.Features.Common.Feedback.Mappings
{
    /// <summary>
    /// Mapper class for feedback entity and DTOs
    /// </summary>
    public static class FeedbackMapper
    {
        public static FeedbackDto ToDto(FeedbackEntity feedback)
        {
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));

            return new FeedbackDto(
                feedback.Id,
                feedback.Title,
                feedback.FeedbackBody,
                feedback.Email?.ToString(),
                feedback.CreatedAt);
        }

    }
}
