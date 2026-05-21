// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the feedback repository
    /// </summary>
    public class FeedbackRepository : RepositoryBase<FeedbackEntity, CommonDbContext>, IFeedbackRepository
    {
        private readonly ILogger<FeedbackRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the feedback repository
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="logger">The logger</param>
        public FeedbackRepository(CommonDbContext dbContext ,ILogger<FeedbackRepository> logger) : base(dbContext)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets all feedback with pagination
        /// </summary>
        /// <param name="page">The page to get</param>
        /// <param name="pageSize">The amount of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<IEnumerable<FeedbackEntity>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            try
            {
                if (page <= 0)
                {
                    _logger.LogWarning("GetAllAsync called with invalid page number: {page}", page);
                    page = 1;
                }
                if (pageSize <= 0)
                {
                    _logger.LogWarning("GetAllAsync called with invalid pageSize: {pageSize}", pageSize);
                    pageSize = 10;
                }

                IQueryable<FeedbackEntity> query = _entities;

                int skip = (page - 1) * pageSize;
                return await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated feedback. Page: {page}, PageSize: {pageSize}", page, pageSize);
                throw;
            }
        }

        /// <summary>
        /// Gets feedback with given ID
        /// </summary>
        /// <param name="id">The id to get</param>
        /// <returns></returns>
        public async Task<FeedbackEntity?> GetFeedbackByIdAsync(Guid id)
        {
            return await _entities.FirstOrDefaultAsync(f => f.Id == id);
        }

        /// <summary>
        /// Gets the total count of feedback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IQueryable<FeedbackEntity> query = _entities;
                return await query.CountAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total count of feedback");
                throw;
            }
        }

        /// <summary>
        /// Checks if feedback exists with the given id
        /// </summary>
        /// <param name="id">Id to be checked</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(x => x.Id == id, cancellationToken);
        }

        /// <summary>
        /// Creates new feedback
        /// </summary>
        /// <param name="feedback">The feedback to be saved</param>
        /// <returns></returns>
        public async Task SaveAsync(FeedbackEntity feedback, CancellationToken cancellationToken)
        {
            try
            {
                if(feedback == null)
                {
                    _logger.LogError("SaveAsync called with null feedback.");
                    throw new ArgumentNullException(nameof(feedback));
                }

            await _entities.AddAsync(feedback, cancellationToken);
                _logger.LogDebug("Created feedback with ID: {id}", feedback.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating feedback with ID: {id}", feedback?.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes feedback based on given id
        /// </summary>
        /// <param name="id">Id of the feedback to be deleted</param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            FeedbackEntity? feedback = await _entities.FindAsync(id);
            if(feedback == null)
            {
                _logger.LogWarning("Could not find feedback with id: {id}", id);
                return false;
            }
            _entities.Remove(feedback);
            return true;
        }
    }
}
