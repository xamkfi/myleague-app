using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballTeamRegisteredEvent by notifying SignalR clients when a team is registered.
    /// </summary>
    public class FloorballTeamRegisteredEventHandler : SignalRDomainEventHandler<FloorballTeamRegisteredEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballTeamRegisteredEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballTeamRegisteredEventHandler(
            FloorballDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballTeamRegisteredEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballTeamRegisteredEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballTeamRegisteredEvent domainEvent)
        {
            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId);

            if (team == null)
            {
                _logger.LogWarning("Floorball team with ID {TeamId} not found for TeamRegistered event.", domainEvent.TeamId);
                return;
            }

            object payload = new
            {
                TeamId = team.Id,
                Name = team.Name,
                RegistrationTime = domainEvent.OccurredOn,
               //TODO:Should team have season here? SeasonId = domainEvent.SeasonId
            };

            _logger.LogInformation("Team registered: {TeamName}", team.Name);

            await NotifyAsync("FloorballTeamRegistered", payload);
        }
    }
} 
