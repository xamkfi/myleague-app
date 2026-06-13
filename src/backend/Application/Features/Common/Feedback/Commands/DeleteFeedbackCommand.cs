// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using MediatR;

namespace Application.Features.Common.Feedback.Commands
{
    /// <summary>
    /// Command for deleting feedback
    /// </summary>
    /// <param name="id"></param>
    public record DeleteFeedbackCommand(Guid id) : IRequest<Result<bool>>;
}
