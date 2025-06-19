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
    /// Handles DivisionActivatedEvent by notifying SignalR clients when a division is activated.
    /// </summary>
    public class DivisionActivatedEventHandler : SignalRDomainEventHandler<DivisionActivatedEvent>
    {
        private readonly CommonDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the DivisionActivatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public DivisionActivatedEventHandler(
            CommonDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<DivisionActivatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the DivisionActivatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(DivisionActivatedEvent domainEvent)
        {
            Division? division = await _dbContext.Divisions
                .FirstOrDefaultAsync(d => d.Id == domainEvent.DivisionId);

            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for DivisionActivated event.", domainEvent.DivisionId);
                return;
            }

            DivisionActivatedNotification notification = new()
            {
                DivisionId = division.Id,
                Name = division.Name,
                SportType = division.SportType,
                ActivatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Division activated: {Name} for {SportType}", division.Name, division.SportType);

            await NotifyAsync("DivisionActivated", notification);
        }
    }
} 