// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.Feedback.DTOs;
using MediatR;

namespace Application.Features.Common.Feedback.Queries
{
    /// <summary>
    /// Query for getting feedback by its id
    /// </summary>
    public record GetFeedbackByIdQuery(Guid id) : IRequest<Result<FeedbackDto>>;
}
