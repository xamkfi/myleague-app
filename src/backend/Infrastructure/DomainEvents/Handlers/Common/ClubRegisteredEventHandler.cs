// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.DomainEvents.Common;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Common
{
    /// <summary>
    /// Handles ClubRegisteredEvent by notifying SignalR clients when a club is registered.
    /// </summary>
    public class ClubRegisteredEventHandler : SignalRDomainEventHandler<ClubRegisteredEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the ClubRegisteredEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public ClubRegisteredEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<ClubRegisteredEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the ClubRegisteredEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(ClubRegisteredEvent domainEvent)
        {
            Club? club = await _dbContext.Clubs
                .FirstOrDefaultAsync(c => c.Id == domainEvent.ClubId);

            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for ClubRegistered event.", domainEvent.ClubId);
                return;
            }

            object payload = new { ClubId = club.Id, Name = club.Name, LogoUrl = club.LogoUrl, RegistrationTime = domainEvent.OccurredOn };

            _logger.LogInformation("Club registered: {Name}", club.Name);

            await NotifyAsync("ClubRegistered", payload);
        }
    }
}
