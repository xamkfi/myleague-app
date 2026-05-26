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
        /// <summary>
        /// Checks if a toggle entity exists 
        /// </summary>
        /// <param name="id">The id of the toggle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the changes to the FeedbackToggle
        /// </summary>
        /// <param name="feedbackToggle">The updated FeedbackToggle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task SaveAsync(FeedbackToggleEntity feedbackToggle, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the first FeedbackToggle in the database if it exists
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task <FeedbackToggleEntity?> GetToggleAsync(CancellationToken cancellationToken);
    }
}
