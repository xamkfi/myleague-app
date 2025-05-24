

namespace Domain.DomainEvents.Hockey
{
    /// <summary>
    /// Event raised when a hockey season is activated
    /// </summary>
    public class HockeySeasonActivatedDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the date and time when the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; }

        /// <summary>
        /// Initializes a new instance of the HockeySeasonActivatedDomainEvent class
        /// </summary>
        /// <param name="seasonId"></param>
        public HockeySeasonActivatedDomainEvent(Guid seasonId)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            SeasonId = seasonId;
        }
    }

    /// <summary>
    /// Event raised when a hockey season is deactivated
    /// </summary>
    public class HockeySeasonDeactivatedDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the date and time when the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; }

        /// <summary>
        /// Initializes a new instance of the HockeySeasonDeactivatedDomainEvent class
        /// </summary>
        /// <param name="seasonId">The ID of the season</param>
        public HockeySeasonDeactivatedDomainEvent(Guid seasonId)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            SeasonId = seasonId;
        }
    }

    /// <summary>
    /// Event raised when a hockey season is completed
    /// </summary>
    public class HockeySeasonCompletedDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the date and time when the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; }

        /// <summary>
        /// Initializes a new instance of the HockeySeasonCompletedDomainEvent class
        /// </summary>
        /// <param name="seasonId">The ID of the season</param>
        public HockeySeasonCompletedDomainEvent(Guid seasonId)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            SeasonId = seasonId;
        }
    }

    /// <summary>
    /// Event raised when a hockey team is added to a season
    /// </summary>
    public class HockeyTeamAddedToSeasonDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the date and time when the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; }

        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Initializes a new instance of the HockeyTeamAddedToSeasonDomainEvent class
        /// </summary>
        /// <param name="seasonId">The ID of the season</param>
        /// <param name="teamId">The ID of the team</param>
        public HockeyTeamAddedToSeasonDomainEvent(Guid seasonId, Guid teamId)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            SeasonId = seasonId;
            TeamId = teamId;
        }
    }

    /// <summary>
    /// Event raised when a hockey team is removed from a season
    /// </summary>
    public class HockeyTeamRemovedFromSeasonDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the date and time when the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; }

        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Initializes a new instance of the HockeyTeamRemovedFromSeasonDomainEvent class
        /// </summary>
        /// <param name="seasonId">The ID of the season</param>
        /// <param name="teamId">The ID of the team</param>
        public HockeyTeamRemovedFromSeasonDomainEvent(Guid seasonId, Guid teamId)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            SeasonId = seasonId;
            TeamId = teamId;
        }
    }
}
