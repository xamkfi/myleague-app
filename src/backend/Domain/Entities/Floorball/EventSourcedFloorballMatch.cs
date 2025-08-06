using Domain.DomainEvents;
using Domain.DomainEvents.Floorball;
using Domain.Enums;
using Domain.Enums.Floorball;
using Domain.EventSourcing;
using Domain.ValueObjects.Floorball;
using System.Collections.Generic;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball match with full event sourcing capabilities
/// </summary>
public class EventSourcedFloorballMatch : EventSourcedAggregate
{
    #region State Properties
    
    /// <summary>
    /// Gets the season ID this match belongs to
    /// </summary>
    public Guid SeasonId { get; private set; }

    /// <summary>
    /// Gets the home team ID
    /// </summary>
    public Guid HomeTeamId { get; private set; }

    /// <summary>
    /// Gets the away team ID
    /// </summary>
    public Guid AwayTeamId { get; private set; }

    /// <summary>
    /// Gets the scheduled date and time of the match
    /// </summary>
    public DateTime ScheduledDateTime { get; private set; }

    /// <summary>
    /// Gets the venue where the match will be played
    /// </summary>
    public string Venue { get; private set; }
    
    /// <summary>
    /// Gets the current status of the match
    /// </summary>
    public FloorballMatchStatus Status { get; private set; }
    
    /// <summary>
    /// Gets the home team's score
    /// </summary>
    public int HomeScore { get; private set; }
    
    /// <summary>
    /// Gets the away team's score
    /// </summary>
    public int AwayScore { get; private set; }

    /// <summary>
    /// Gets whether the match went to overtime
    /// </summary>
    public bool WentToOvertime { get; private set; }

    /// <summary>
    /// Gets whether the match went to shootout
    /// </summary>
    public bool WentToShootout { get; private set; }
    
    /// <summary>
    /// Gets all goal events
    /// </summary>
    public IReadOnlyCollection<FloorballGoalScoredEvent> GoalEvents => _goalEvents.AsReadOnly();
    private readonly List<FloorballGoalScoredEvent> _goalEvents = new();
    
    /// <summary>
    /// Gets all penalty events
    /// </summary>
    public IReadOnlyCollection<FloorballPenaltyAssignedEvent> PenaltyEvents => _penaltyEvents.AsReadOnly();
    private readonly List<FloorballPenaltyAssignedEvent> _penaltyEvents = new();
    
    /// <summary>
    /// Gets the match officials (referees)
    /// </summary>
    public IReadOnlyCollection<Guid> OfficialIds => _officialIds.AsReadOnly();
    private readonly List<Guid> _officialIds = new();
    
    /// <summary>
    /// Gets the period scores (per period)
    /// </summary>
    public IReadOnlyDictionary<int, (int HomeScore, int AwayScore)> PeriodScores => _periodScores;
    private readonly Dictionary<int, (int HomeScore, int AwayScore)> _periodScores = new();

    /// <summary>
    /// Public constructor for event replay
    /// </summary>
    public EventSourcedFloorballMatch()
    {
        // Initialize period scores for standard 3 periods in floorball
        for (int i = 1; i <= 3; i++)
        {
            _periodScores[i] = (0, 0);
        }
        Venue = string.Empty;
    }
    
    /// <summary>
    /// Creates a new floorball match
    /// </summary>
    /// <param name="id">The match ID</param>
    /// <param name="seasonId">The season ID</param>
    /// <param name="homeTeamId">The home team ID</param>
    /// <param name="awayTeamId">The away team ID</param>
    /// <param name="scheduledDateTime">The scheduled date and time</param>
    /// <param name="venue">The venue</param>
    /// <exception cref="ArgumentException">Thrown when home and away teams are the same</exception>
    public static EventSourcedFloorballMatch Create(
        Guid id,
        Guid seasonId,
        Guid homeTeamId,
        Guid awayTeamId,
        DateTime scheduledDateTime,
        string venue)
    {
        if (homeTeamId == awayTeamId)
            throw new ArgumentException("Home team and away team cannot be the same.");
        if (string.IsNullOrWhiteSpace(venue))
            throw new ArgumentException("Venue cannot be null or empty.", nameof(venue));

        // Ensure the scheduled date is stored as UTC to match PostgreSQL 'timestamp with time zone'
        DateTime utcScheduled = scheduledDateTime.Kind switch
        {
            DateTimeKind.Utc => scheduledDateTime,
            DateTimeKind.Local => scheduledDateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(scheduledDateTime, DateTimeKind.Utc)
        };

        EventSourcedFloorballMatch match = new EventSourcedFloorballMatch();
        FloorballMatchCreatedEvent createdEvent = new FloorballMatchCreatedEvent(
            id,
            seasonId,
            homeTeamId,
            awayTeamId,
            utcScheduled,
            venue);
        match.ApplyEvent(createdEvent);
        return match;
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Reschedules the match
    /// </summary>
    /// <param name="newDateTime">The new date and time</param>
    /// <param name="newVenue">The new venue (optional)</param>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow rescheduling</exception>
    public void Reschedule(DateTime newDateTime, string? newVenue = null)
    {
        if (Status != FloorballMatchStatus.Scheduled && Status != FloorballMatchStatus.Postponed)
            throw new InvalidOperationException($"Cannot reschedule a match with status {Status}.");
        
        FloorballMatchRescheduledEvent rescheduledEvent = new FloorballMatchRescheduledEvent(
            Id,
            ScheduledDateTime,
            newDateTime,
            Venue,
            newVenue ?? Venue);
        
        ApplyEvent(rescheduledEvent);
    }

    /// <summary>
    /// Postpones the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow postponing</exception>
    public void Postpone()
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot postpone a match with status {Status}.");
        
        FloorballMatchStatusChangedEvent statusChangedEvent = new FloorballMatchStatusChangedEvent(
            Id,
            Status,
            FloorballMatchStatus.Postponed);
        
        ApplyEvent(statusChangedEvent);
    }

    /// <summary>
    /// Starts the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow starting</exception>
    public void Start()
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start a match with status {Status}.");
        
        if (_officialIds.Count == 0)
            throw new InvalidOperationException("Cannot start a match without officials.");
        
        FloorballMatchStatusChangedEvent statusChangedEvent = new FloorballMatchStatusChangedEvent(
            Id,
            Status,
            FloorballMatchStatus.InProgress);
        
        ApplyEvent(statusChangedEvent);
        
        // Also emit a match started event with more details
        FloorballMatchStartedEvent matchStartedEvent = new FloorballMatchStartedEvent(
            Id,
            DateTime.UtcNow);
        
        ApplyEvent(matchStartedEvent);

        StartPeriod(1);
    }

    /// <summary>
    /// Records a goal
    /// </summary>
    /// <param name="scoringTeamId">The ID of the team that scored</param>
    /// <param name="scoringPlayerId">The ID of the player who scored</param>
    /// <param name="periodNumber">The period number (1-5)</param>
    /// <param name="timeInSeconds">The time in seconds when the goal was scored</param>
    /// <param name="wasInOvertime">Whether the goal was scored in overtime</param>
    /// <param name="wasInShootout">Whether the goal was scored in shootout</param>
    /// <param name="assistingPlayerId">The ID of the player who assisted (optional)</param>
    /// <param name="secondaryAssistingPlayerId">The ID of the second player who assisted (optional)</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public void RecordGoal(
        Guid scoringTeamId,
        Guid scoringPlayerId,
        int periodNumber,
        int timeInSeconds,
        bool wasInOvertime = false,
        bool wasInShootout = false,
        Guid? assistingPlayerId = null,
        Guid? secondaryAssistingPlayerId = null)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot record a goal for a match with status {Status}.");
        if (_periodScores.Count == 0 || periodNumber < 1 || periodNumber > _periodScores.Count)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), $"Period number must be between 1 and {_periodScores.Count}.");
        if (timeInSeconds < 0 || timeInSeconds > 1200)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be between 0 and 1200 seconds.");
        if (scoringTeamId != HomeTeamId && scoringTeamId != AwayTeamId)
            throw new ArgumentException("Scoring team must be either the home team or the away team.", nameof(scoringTeamId));
        
        FloorballGoalScoredEvent goalScoredEvent = new FloorballGoalScoredEvent(
            Id,
            scoringTeamId,
            scoringPlayerId,
            periodNumber,
            timeInSeconds,
            wasInOvertime,
            false, // isPenaltyShot - not implemented yet
            wasInShootout,
            assistingPlayerId,
            secondaryAssistingPlayerId);
        ApplyEvent(goalScoredEvent);
    }

    /// <summary>
    /// Records a penalty
    /// </summary>
    /// <param name="teamId">The ID of the team that received the penalty</param>
    /// <param name="playerId">The ID of the player who received the penalty</param>
    /// <param name="penaltyType">The type of penalty</param>
    /// <param name="minutes">The duration of the penalty in minutes</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds when the penalty was given</param>
    /// <param name="description">Description of the penalty</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public void RecordPenalty(
        Guid teamId,
        Guid? playerId,
        FloorballPenaltyType penaltyType,
        int minutes,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot record a penalty for a match with status {Status}.");
        if (_periodScores.Count == 0 || periodNumber < 1 || periodNumber > _periodScores.Count)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), $"Period number must be between 1 and {_periodScores.Count}.");
        if (timeInSeconds < 0 || timeInSeconds > 1200)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be between 0 and 1200 seconds.");
        if (minutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(minutes), "Penalty minutes must be positive.");
        if (teamId != HomeTeamId && teamId != AwayTeamId)
            throw new ArgumentException("Team must be either the home team or the away team.", nameof(teamId));
        FloorballPenaltyAssignedEvent penaltyAssignedEvent = new FloorballPenaltyAssignedEvent(
            Id,
            teamId,
            playerId,
            penaltyType,
            minutes,
            periodNumber,
            timeInSeconds,
            description ?? string.Empty);
        ApplyEvent(penaltyAssignedEvent);
    }

    /// <summary>
    /// Adds an official (referee) to the match
    /// </summary>
    /// <param name="refereeId">The ID of the referee to add</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in a state that allows adding officials</exception>
    public void AddOfficial(Guid refereeId)
    {
        if (Status != FloorballMatchStatus.Scheduled && Status != FloorballMatchStatus.Postponed)
            throw new InvalidOperationException($"Cannot add officials to a match with status {Status}.");
        
        if (_officialIds.Contains(refereeId))
            return;
        
        FloorballOfficialAssignedEvent officialAssignedEvent = new FloorballOfficialAssignedEvent(
            Id,
            refereeId);
            
        ApplyEvent(officialAssignedEvent);
    }

    /// <summary>
    /// Records that the match went to overtime
    /// </summary>
    public void RecordOvertime()
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot record overtime for a match with status {Status}.");
        
        if (WentToOvertime)
            return; // Already in overtime
        
        FloorballMatchOvertimeStartedEvent overtimeStartedEvent = new FloorballMatchOvertimeStartedEvent(Id);
        ApplyEvent(overtimeStartedEvent);
    }

    /// <summary>
    /// Records that the match went to shootout
    /// </summary>
    public void RecordShootout()
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot record shootout for a match with status {Status}.");
        
        if (!WentToOvertime)
            throw new InvalidOperationException("Match must go to overtime before shootout.");
        
        if (WentToShootout)
            return; // Already in shootout
        
        FloorballMatchShootoutStartedEvent shootoutStartedEvent = new FloorballMatchShootoutStartedEvent(Id);
        ApplyEvent(shootoutStartedEvent);
    }

    /// <summary>
    /// Completes the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public void Complete()
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a match with status {Status}.");
        
        FloorballMatchStatusChangedEvent statusChangedEvent = new FloorballMatchStatusChangedEvent(
            Id,
            Status,
            FloorballMatchStatus.Completed);
        
        ApplyEvent(statusChangedEvent);
        
        // Also emit a match completed event with the final score
        FloorballMatchCompletedEvent matchCompletedEvent = new FloorballMatchCompletedEvent(
            Id,
            HomeScore,
            AwayScore,
            WentToOvertime,
            WentToShootout);
        
        ApplyEvent(matchCompletedEvent);

        EndPeriod(3);
    }

    /// <summary>
    /// Cancels the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match is already completed</exception>
    public void Cancel()
    {
        if (Status == FloorballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed match.");
        
        FloorballMatchStatusChangedEvent statusChangedEvent = new FloorballMatchStatusChangedEvent(
            Id,
            Status,
            FloorballMatchStatus.Cancelled);
        
        ApplyEvent(statusChangedEvent);
    }

    /// <summary>
    /// Changes the season of the match
    /// </summary>
    /// <param name="newSeasonId">The new season ID</param>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow changing season</exception>
    public void ChangeSeason(Guid newSeasonId)
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot change season for a match with status {Status}.");
        
        if (SeasonId == newSeasonId)
            return; // No change needed
        
        FloorballMatchSeasonChangedEvent seasonChangedEvent = new FloorballMatchSeasonChangedEvent(
            Id,
            SeasonId,
            newSeasonId);
        
        ApplyEvent(seasonChangedEvent);
    }

    /// <summary>
    /// Changes the teams of the match
    /// </summary>
    /// <param name="newHomeTeamId">The new home team ID</param>
    /// <param name="newAwayTeamId">The new away team ID</param>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow changing teams</exception>
    public void ChangeTeams(Guid newHomeTeamId, Guid newAwayTeamId)
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot change teams for a match with status {Status}.");
        
        if (newHomeTeamId == newAwayTeamId)
            throw new ArgumentException("Home team and away team cannot be the same.", nameof(newAwayTeamId));
        
        if (HomeTeamId == newHomeTeamId && AwayTeamId == newAwayTeamId)
            return; // No change needed
        
        FloorballMatchTeamsChangedEvent teamsChangedEvent = new FloorballMatchTeamsChangedEvent(
            Id,
            HomeTeamId,
            newHomeTeamId,
            AwayTeamId,
            newAwayTeamId);
        
        ApplyEvent(teamsChangedEvent);
    }

    /// <summary>
    /// Changes the venue of the match
    /// </summary>
    /// <param name="newVenue">The new venue</param>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow changing venue</exception>
    public void ChangeVenue(string newVenue)
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot change venue for a match with status {Status}.");
        
        if (string.IsNullOrWhiteSpace(newVenue))
            throw new ArgumentException("Venue cannot be null or empty.", nameof(newVenue));
        
        if (Venue == newVenue)
            return; // No change needed
        
        FloorballMatchVenueChangedEvent venueChangedEvent = new FloorballMatchVenueChangedEvent(
            Id,
            Venue,
            newVenue);
        
        ApplyEvent(venueChangedEvent);
    }

    public void EndPeriod(int periodNumber)
    {
        bool isLastRegularPeriod = periodNumber == 3;
        FloorballPeriodEndedEvent floorballPeriodEndedEvent = new FloorballPeriodEndedEvent(
            Id,
            periodNumber,
            HomeScore,
            AwayScore,
            isLastRegularPeriod);

        ApplyEvent(floorballPeriodEndedEvent);
    }

    public void StartPeriod(int periodNumber)
    {
        bool isLastRegularPeriod = periodNumber == 3;
        FloorballPeriodStartedEvent floorballPeriodStartedEvent = new FloorballPeriodStartedEvent(
            Id,
            periodNumber,
            HomeScore,
            AwayScore,
            isLastRegularPeriod);

        ApplyEvent(floorballPeriodStartedEvent);
    }
    #endregion
    
    #region Apply Methods
    
    // These methods are called by the base class via reflection when applying events
    
    /// <summary>
    /// Applies a match created event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchCreatedEvent @event)
    {
        Id = @event.MatchId;
        SeasonId = @event.SeasonId;
        HomeTeamId = @event.HomeTeamId;
        AwayTeamId = @event.AwayTeamId;
        ScheduledDateTime = @event.ScheduledDateTime;
        Venue = @event.Venue ?? string.Empty;
        Status = FloorballMatchStatus.Scheduled;
        HomeScore = 0;
        AwayScore = 0;
        WentToOvertime = false;
        WentToShootout = false;
    }
    
    /// <summary>
    /// Applies a match rescheduled event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchRescheduledEvent @event)
    {
        ScheduledDateTime = @event.NewScheduledDateTime;
        Venue = @event.NewVenue;
    }
    
    /// <summary>
    /// Applies a match status changed event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchStatusChangedEvent @event)
    {
        Status = @event.NewStatus;
    }
    
    /// <summary>
    /// Applies a match started event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private static void Apply(FloorballMatchStartedEvent @event)
    {
        // No additional state changes beyond the status change
    }
    
    /// <summary>
    /// Applies a goal scored event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballGoalScoredEvent @event)
    {
        // Add the existing event to our collection (don't create a new one)
        _goalEvents.Add(@event);
        
        // Update scores
        if (@event.TeamId == HomeTeamId)
        {
            HomeScore++;

            (int HomeScore, int AwayScore) currentPeriodScore = _periodScores[@event.PeriodNumber];
            _periodScores[@event.PeriodNumber] = (currentPeriodScore.HomeScore + 1, currentPeriodScore.AwayScore);
        }
        else if (@event.TeamId == AwayTeamId)
        {
            AwayScore++;
            
            (int HomeScore, int AwayScore) currentPeriodScore = _periodScores[@event.PeriodNumber];
            _periodScores[@event.PeriodNumber] = (currentPeriodScore.HomeScore, currentPeriodScore.AwayScore + 1);
        }
    }
    
    /// <summary>
    /// Applies a penalty assigned event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballPenaltyAssignedEvent @event)
    {
        // Add the existing event to our collection (don't create a new one)
        _penaltyEvents.Add(@event);
    }
    
    /// <summary>
    /// Applies an official assigned event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballOfficialAssignedEvent @event)
    {
        _officialIds.Add(@event.RefereeId);
    }
    
    /// <summary>
    /// Applies a match overtime started event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchOvertimeStartedEvent @event)
    {
        WentToOvertime = true;
        
        // Add an additional period for overtime if it doesn't exist yet
        if (!_periodScores.ContainsKey(4))
        {
            _periodScores[4] = (0, 0);
        }
    }
    
    /// <summary>
    /// Applies a match shootout started event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchShootoutStartedEvent @event)
    {
        WentToShootout = true;
        
        // Add an additional period for shootout if it doesn't exist yet
        if (!_periodScores.ContainsKey(5))
        {
            _periodScores[5] = (0, 0);
        }
    }
    
    /// <summary>
    /// Applies a match completed event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private static void Apply(FloorballMatchCompletedEvent @event)
    {
        // The status change is handled by the status changed event
        // This event just contains the final score which we already track
    }
    
    /// <summary>
    /// Applies a match season changed event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchSeasonChangedEvent @event)
    {
        SeasonId = @event.NewSeasonId;
    }
    
    /// <summary>
    /// Applies a match teams changed event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchTeamsChangedEvent @event)
    {
        HomeTeamId = @event.NewHomeTeamId;
        AwayTeamId = @event.NewAwayTeamId;
    }
    
    /// <summary>
    /// Applies a match venue changed event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballMatchVenueChangedEvent @event)
    {
        Venue = @event.NewVenue ?? string.Empty;
    }
    
    /// <summary>
    /// Applies a period started event
    /// </summary>
    /// <param name="event">The event to apply</param>
    private void Apply(FloorballPeriodStartedEvent @event)
    {
        // Add the period to period scores if it doesn't exist yet
        if (!_periodScores.ContainsKey(@event.PeriodNumber))
        {
            _periodScores[@event.PeriodNumber] = (0, 0);
        }
    }
    
    #endregion
} 
