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
    public class FeedbackToggleRepository : RepositoryBase<FeedbackToggle, CommonDbContext>, IFeedbackToggleRepository
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

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _entities.AnyAsync(ft => ft.Id == id,, cancellationToken);
        }

        public async Task SaveAsync(FeedbackToggle toggle, CancellationToken cancellationToken)
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
                    await _entities.AddAsync(toggle);
                    _logger.LogDebug("Added the toggle to the database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving the toggle state");
            }
        }
    }
}
