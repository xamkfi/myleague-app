using Domain.DomainEvents.Floorball;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.DTOs.Notifications;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;
using MyLeague.Infrastructure.SignalR.Sports.Floorball;
using System.Threading;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballOfficialAssignedEvent by notifying SignalR clients with official assignment details.
    /// </summary>
    public class FloorballOfficialAssignedEventHandler : NotificationDomainEventHandler<FloorballOfficialAssignedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballOfficialAssignedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballOfficialAssignedEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballOfficialAssignedEventHandler> logger)
            : base(notificationSender, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Builds the notification payload from the domain event
        /// </summary>
        /// <param name="domainEvent">The domain event</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A tuple containing the event name and notification payload</returns>
        protected override async Task<(string EventName, object? Notification)> BuildNotificationAsync(
            FloorballOfficialAssignedEvent domainEvent, 
            CancellationToken cancellationToken = default)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId, cancellationToken);

            FloorballReferee? referee = await _dbContext.Set<FloorballReferee>()
                .Include(r => r.Person)
                .FirstOrDefaultAsync(r => r.Id == domainEvent.RefereeId, cancellationToken);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for OfficialAssigned event.", domainEvent.MatchId);
                return (FloorballNotificationEvents.OfficialAssigned, null);
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";
            string officialName = referee?.Person?.FullName ?? "Unknown Official";

            // Create team info objects first
            TeamInfo homeTeamInfo = new TeamInfo
            {
                Id = match.HomeTeam?.Id ?? Guid.Empty,
                Name = homeTeamName
            };

            TeamInfo awayTeamInfo = new TeamInfo
            {
                Id = match.AwayTeam?.Id ?? Guid.Empty,
                Name = awayTeamName
            };

            FloorballOfficialAssignedNotification notification = new FloorballOfficialAssignedNotification
            {
                MatchId = domainEvent.MatchId,
                RefereeId = domainEvent.RefereeId,
                OfficialName = officialName,
                HomeTeam = homeTeamInfo,
                AwayTeam = awayTeamInfo,
                ScheduledDateTime = match.ScheduledDateTime,
                AssignedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Official {OfficialName} assigned to match between {HomeTeam} and {AwayTeam}", 
                officialName, homeTeamName, awayTeamName);

            return (FloorballNotificationEvents.OfficialAssigned, notification);
        }
    }
} 