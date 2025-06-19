// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.DomainEvents.Common;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.DTOs.Notifications;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Common
{
    /// <summary>
    /// Handles DivisionUpdatedEvent by notifying SignalR clients when a division is updated.
    /// </summary>
    public class DivisionUpdatedEventHandler : SignalRDomainEventHandler<DivisionUpdatedEvent>
    {
        private readonly CommonDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the DivisionUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public DivisionUpdatedEventHandler(
            CommonDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<DivisionUpdatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the DivisionUpdatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(DivisionUpdatedEvent domainEvent)
        {
            Division? division = await _dbContext.Divisions
                .FirstOrDefaultAsync(d => d.Id == domainEvent.DivisionId);

            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for DivisionUpdated event.", domainEvent.DivisionId);
                return;
            }

            DivisionUpdatedNotification notification = new()
            {
                DivisionId = division.Id,
                Name = division.Name,
                Description = division.Description,
                Level = division.Level,
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Division updated: {Name} (Level: {Level})", division.Name, division.Level);

            await NotifyAsync("DivisionUpdated", notification);
        }
    }
} 