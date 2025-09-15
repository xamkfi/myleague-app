using Domain.DomainEvents.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.DTOs.Notifications;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;
using MyLeague.Infrastructure.SignalR.Sports.Floorball;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Floorball;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    public class FloorballSaveEventHandler : NotificationDomainEventHandler<FloorballSaveEvent>
    {
        private readonly FloorballDbContext _dbContext;

        public FloorballSaveEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballSaveEventHandler> logger)
            : base(notificationSender, logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<(string EventName, object? Notification)> BuildNotificationAsync(
            FloorballSaveEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballPlayer? goalie = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.GoalieId, cancellationToken);

            if (goalie is null)
            {
                _logger.LogWarning("Goalie with ID {GoalieId} not found for Save event.", domainEvent.GoalieId);
                return (FloorballNotificationEvents.SaveRecorded, null);
            }

            FloorballSaveNotification notification = new FloorballSaveNotification
            {
                MatchId = domainEvent.MatchId,
                TeamId = domainEvent.TeamId,
                GoalieId = domainEvent.GoalieId,
                GoalieName = $"{goalie.Person.FirstName} {goalie.Person.LastName}",
                PeriodNumber = domainEvent.PeriodNumber,
                TimeInSeconds = domainEvent.TimeInSeconds,
                IsOvertime = domainEvent.WasInOvertime,
                IsShootout = domainEvent.WasInShootout
            };

            return (FloorballNotificationEvents.SaveRecorded, notification);
        }
    }
}
