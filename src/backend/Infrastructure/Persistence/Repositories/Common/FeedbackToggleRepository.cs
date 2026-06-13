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
    /// Implementation of the FeedbackToggleRepository
    /// </summary>
    public class FeedbackToggleRepository : RepositoryBase<FeedbackToggleEntity, CommonDbContext>, IFeedbackToggleRepository
    {
        private readonly ILogger<FeedbackToggleRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the FeedbackToggleRepository
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="logger">The logger</param>
        public FeedbackToggleRepository(CommonDbContext dbContext,  ILogger<FeedbackToggleRepository> logger) : base(dbContext)
        {
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(ft => ft.Id == id, cancellationToken);
        }

        /// <summary>
        /// Saves the FeedbackToggle
        /// </summary>
        /// <param name="toggle">The toggle with the values to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SaveAsync(FeedbackToggleEntity toggle, CancellationToken cancellationToken = default)
        {
            try
            {
                if(toggle == null)
                {
                    _logger.LogError("ToggleAsync called with null toggle entity");
                    throw new ArgumentNullException(nameof(toggle));
                }

                bool exists = await ExistsAsync(toggle.Id, cancellationToken);
                if (exists)
                {
                    _entities.Update(toggle);
                    _logger.LogDebug("Updated FeedbackToggle state to {isEnabled}", toggle.IsEnabled);
                }
                else
                {
                    await _entities.AddAsync(toggle, cancellationToken);
                    _logger.LogDebug("Added the toggle to the database with ID: {id}", toggle.Id);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while saving the toggle state");
            }
        }

        /// <summary>
        /// Gets the FeedbackToggle if it exists
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<FeedbackToggleEntity?> GetToggleAsync(CancellationToken cancellationToken = default)
        {
            FeedbackToggleEntity? toggle = await _entities.FirstOrDefaultAsync(cancellationToken);
            if(toggle == null)
            {
                _logger.LogWarning("No toggle found in the database.");
                return null;
            }
            _logger.LogInformation("Successfully retrieved toggle with state: {toggle.IsEnabled}", toggle.IsEnabled);
            return toggle;
        }
    }
}
