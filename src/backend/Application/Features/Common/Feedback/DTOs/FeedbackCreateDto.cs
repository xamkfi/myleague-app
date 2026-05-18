// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Common.Feedback.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating new Feedback
    /// </summary>
    public record FeedbackCreateDto(
        string Title,
        string FeedbackBody,
        string? Email);
}
