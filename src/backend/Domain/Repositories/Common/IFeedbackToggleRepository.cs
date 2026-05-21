// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Domain.Repositories.Common
{
    public interface IFeedbackToggleRepository
    {
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        public Task SaveAsync(FeedbackToggle feedbackToggle, CancellationToken cancellationToken);
    }
}
