using Domain.DomainEvents.Common;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Common
{
    /// <summary>
    /// Handles ClubInfoUpdatedEvent by notifying SignalR clients when a club's information is updated.
    /// </summary>
    public class ClubInfoUpdatedEventHandler : SignalRDomainEventHandler<ClubInfoUpdatedEvent>
    {
        private readonly CommonDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the ClubInfoUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public ClubInfoUpdatedEventHandler(
            CommonDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<ClubInfoUpdatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the ClubInfoUpdatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(ClubInfoUpdatedEvent domainEvent)
        {
            Club? club = await _dbContext.Clubs
                .FirstOrDefaultAsync(c => c.Id == domainEvent.ClubId);

            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for ClubInfoUpdated event.", domainEvent.ClubId);
                return;
            }

            object payload = new { 
                ClubId = club.Id, 
                Name = club.Name, 
                LogoUrl = club.LogoUrl, 
                UpdatedOn = domainEvent.OccurredOn 
            };

            _logger.LogInformation("Club information updated: {Name}", club.Name);

            await NotifyAsync("ClubInfoUpdated", payload);
        }
    }
} 