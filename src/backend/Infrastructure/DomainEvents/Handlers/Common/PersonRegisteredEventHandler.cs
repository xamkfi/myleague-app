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
    /// Handles PersonRegisteredEvent by notifying SignalR clients when a person is registered.
    /// </summary>
    public class PersonRegisteredEventHandler : SignalRDomainEventHandler<PersonRegisteredEvent>
    {
        private readonly CommonDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the PersonRegisteredEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public PersonRegisteredEventHandler(
            CommonDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<PersonRegisteredEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the PersonRegisteredEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(PersonRegisteredEvent domainEvent)
        {
            Person? person = await _dbContext.Persons.FirstOrDefaultAsync(p => p.Id == domainEvent.PersonId);

            if (person == null)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for PersonRegistered event.", domainEvent.PersonId);
                return;
            }

            object payload = new { PersonId = person.Id, FirstName = person.FirstName, LastName = person.LastName, RegistrationTime = domainEvent.OccurredOn };

            _logger.LogInformation("Person registered: {FirstName} {LastName}", person.FirstName, person.LastName);

            await NotifyAsync("PersonRegistered", payload);
        }
    }
}
