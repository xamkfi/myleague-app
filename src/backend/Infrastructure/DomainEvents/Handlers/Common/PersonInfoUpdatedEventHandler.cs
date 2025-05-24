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
    /// Handles PersonInfoUpdatedEvent by notifying SignalR clients when a person's information is updated.
    /// </summary>
    public class PersonInfoUpdatedEventHandler : SignalRDomainEventHandler<PersonInfoUpdatedEvent>
    {
        private readonly CommonDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the PersonInfoUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public PersonInfoUpdatedEventHandler(
            CommonDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<PersonInfoUpdatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the PersonInfoUpdatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(PersonInfoUpdatedEvent domainEvent)
        {
            Person? person = await _dbContext.Persons.FirstOrDefaultAsync(p => p.Id == domainEvent.PersonId);

            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for PersonInfoUpdated event.", domainEvent.PersonId);
                return;
            }

            object payload = new { PersonId = person.Id, FirstName = person.FirstName, LastName = person.LastName, UpdatedOn = domainEvent.OccurredOn };

            _logger.LogInformation("Person information updated: {FirstName} {LastName}", person.FirstName, person.LastName);

            await NotifyAsync("PersonInfoUpdated", payload);
        }
    }
}
