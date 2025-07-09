// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection for updating Match status to completed
    /// </summary>
    public sealed class FloorballMatchCompletedProjection: IDomainEventHandler<FloorballMatchCompletedEvent>
    {
        private readonly FloorballDbContext _floorballDbContext;
        private readonly ILogger<FloorballMatchCompletedProjection> _logger;

        public FloorballMatchCompletedProjection(
            FloorballDbContext floorballDbContext,
            ILogger<FloorballMatchCompletedProjection> logger)
        {
            _floorballDbContext = floorballDbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updating match status to completed
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public async Task HandleAsync(FloorballMatchCompletedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballMatchCompletedEvent for match {MatchId}", domainEvent.MatchId);

            try
            {
                FloorballMatch? match = await _floorballDbContext.FloorballMatches
                    .Include(m => m.Officials)
                    .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

                if (match == null)
                {
                    _logger.LogWarning("Match not found – FloorballMatch {MatchId}", domainEvent.MatchId);
                    return;
                }

                if (match.Status == Domain.Enums.Floorball.FloorballMatchStatus.Completed)
                {
                    _logger.LogDebug("Match {MatchId} already completed", domainEvent.MatchId);
                    return;
                }

                match.Complete();
                await _floorballDbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully completed match {MatchId}", domainEvent.MatchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed while completing FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
}
