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
    /// Handles DivisionCreatedEvent by notifying SignalR clients when a division is created.
    /// </summary>
    public class DivisionCreatedEventHandler : SignalRDomainEventHandler<DivisionCreatedEvent>
    {
        private readonly CommonDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the DivisionCreatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public DivisionCreatedEventHandler(
            CommonDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<DivisionCreatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the DivisionCreatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(DivisionCreatedEvent domainEvent)
        {
            Division? division = await _dbContext.Divisions
                .FirstOrDefaultAsync(d => d.Id == domainEvent.DivisionId);

            if (division == null)
            {
                _logger.LogWarning("Division with ID {DivisionId} not found for DivisionCreated event.", domainEvent.DivisionId);
                return;
            }

            DivisionCreatedNotification notification = new()
            {
                DivisionId = division.Id,
                Name = division.Name,
                Description = division.Description,
                Level = division.Level,
                SportType = division.SportType,
                CreatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Division created: {Name} for {SportType}", division.Name, division.SportType);

            await NotifyAsync("DivisionCreated", notification);
        }
    }
} 