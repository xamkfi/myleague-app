// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.FeedbackToggle.DTOs;
using MediatR;

namespace Application.Features.Common.FeedbackToggle.Queries
{
    /// <summary>
    /// Query to get FeedbackToggle
    /// </summary>
    public record GetFeedbackToggleQuery() : IRequest<Result<FeedbackToggleDto>>;
}
