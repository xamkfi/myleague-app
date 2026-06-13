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
    public interface IFeedbackRepository
    {
        /// <summary>
        /// Gets all feedback with pagination
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Amount of items per page</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task<IEnumerable<FeedbackEntity>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the total amount of feedback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets feedback with given ID
        /// </summary>
        /// <param name="id">The id to get</param>
        /// <returns></returns>
        Task<FeedbackEntity?> GetFeedbackByIdAsync(Guid id);

        /// <summary>
        ///  Checks if feedback exists with the given id
        /// </summary>
        /// <param name="id">Id to be checked</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves new feedback
        /// </summary>
        /// <param name="feedback">Feedback entity to be saved</param>
        /// <returns></returns>
        Task SaveAsync(FeedbackEntity feedback, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes feedback based on given id
        /// </summary>
        /// <param name="id">Id of the feedback to be deleted</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
