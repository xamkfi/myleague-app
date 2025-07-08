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

        public async Task HandleAsync(FloorballMatchCompletedEvent domainEvent)
        {
            FloorballMatch? match = await _floorballDbContext.FloorballMatches
                .Include(m => m.Officials)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
                return;

            match.Complete();
            await _floorballDbContext.SaveChangesAsync();
        }
    }
}
