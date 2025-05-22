using Domain.Enums;
using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;
using System.Collections.Generic;
using Domain.EventSourcing;
using Domain.DomainEvents.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball match
/// </summary>
public class FloorballMatch : AggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of the match
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the season this match belongs to
    /// </summary>
    public FloorballSeason Season { get; private set; }

    /// <summary>
    /// Gets or sets the ID of the season
    /// </summary>
    public Guid SeasonId { get; private set; }

    /// <summary>
    /// Gets the home team
    /// </summary>
    public FloorballTeam HomeTeam { get; private set; }
    
    /// <summary>
    /// Gets the ID of the home team
    /// </summary>
    public Guid HomeTeamId { get; private set; }

    /// <summary>
    /// Gets the away team
    /// </summary>
    public FloorballTeam AwayTeam { get; private set; }
    
    /// <summary>
    /// Gets the ID of the away team
    /// </summary>
    public Guid AwayTeamId { get; private set; }

    /// <summary>
    /// Gets the scheduled date and time of the match
    /// </summary>
    public DateTime ScheduledDateTime { get; private set; }

    /// <summary>
    /// Gets the venue where the match will be played
    /// </summary>
    public string? Venue { get; private set; }
    
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
    /// Gets all match events (goals, penalties, etc.)
    /// </summary>
    public IReadOnlyCollection<FloorballMatchEventBase> Events => _events.AsReadOnly();
    private readonly List<FloorballMatchEventBase> _events = new();
    
    /// <summary>
    /// Gets all goal events
    /// </summary>
    public IReadOnlyCollection<FloorballGoalEvent> GoalEvents => 
        _events.OfType<FloorballGoalEvent>().ToList().AsReadOnly();
    
    /// <summary>
    /// Gets all penalty events
    /// </summary>
    public IReadOnlyCollection<FloorballPenaltyEvent> PenaltyEvents => 
        _events.OfType<FloorballPenaltyEvent>().ToList().AsReadOnly();
    
    /// <summary>
    /// Gets the match officials (referees)
    /// </summary>
    public IReadOnlyCollection<FloorballReferee> Officials => _officials.AsReadOnly();
    private readonly List<FloorballReferee> _officials = new();
    
    /// <summary>
    /// Gets the period scores (per period)
    /// </summary>
    public IReadOnlyCollection<FloorballPeriodScore> PeriodScores => _periodScores.AsReadOnly();
    private readonly List<FloorballPeriodScore> _periodScores = new();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballMatch()
    {
        Id = Guid.NewGuid();
        Status = FloorballMatchStatus.Scheduled;
        HomeScore = 0;
        AwayScore = 0;
        WentToOvertime = false;
        WentToShootout = false;
        _events = new List<FloorballMatchEventBase>();
        _officials = new List<FloorballReferee>();
        _periodScores = new List<FloorballPeriodScore>();
        Season = null!; // EF Core will set this
        HomeTeam = null!;
        AwayTeam = null!;
        Venue = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballMatch class
    /// </summary>
    /// <param name="season">The season this match belongs to</param>
    /// <param name="homeTeam">The home team</param>
    /// <param name="awayTeam">The away team</param>
    /// <param name="scheduledDateTime">The scheduled date and time of the match</param>
    /// <param name="venue">The venue where the match will be played</param>
    /// <exception cref="ArgumentNullException">Thrown when a required parameter is null</exception>
    /// <exception cref="ArgumentException">Thrown when teams are the same or venue is invalid</exception>
    public FloorballMatch(
        FloorballSeason season,
        FloorballTeam homeTeam,
        FloorballTeam awayTeam,
        DateTime scheduledDateTime,
        string? venue)
    {
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(homeTeam);
        ArgumentNullException.ThrowIfNull(awayTeam);

        if (homeTeam == awayTeam)
            throw new ArgumentException("Home team and away team cannot be the same team.");

        Id = Guid.NewGuid();
        Season = season;
        SeasonId = season.Id;
        HomeTeam = homeTeam;
        HomeTeamId = homeTeam.Id;
        AwayTeam = awayTeam;
        AwayTeamId = awayTeam.Id;
        ScheduledDateTime = scheduledDateTime;
        Venue = venue;
        Status = FloorballMatchStatus.Scheduled;
        HomeScore = 0;
        AwayScore = 0;
        WentToOvertime = false;
        WentToShootout = false;
        _events = new List<FloorballMatchEventBase>();
        _officials = new List<FloorballReferee>();
        _periodScores = new List<FloorballPeriodScore>();
        for (int i = 1; i <= 3; i++)
        {
            _periodScores.Add(new FloorballPeriodScore(i, 0, 0));
        }
    }

    /// <summary>
    /// Sets the season for this match
    /// </summary>
    /// <param name="season">The season to set</param>
    /// <exception cref="ArgumentNullException">Thrown when the season is null</exception>
    public void SetSeason(FloorballSeason season)
    {
        ArgumentNullException.ThrowIfNull(season);
        Season = season;
        SeasonId = season.Id;
    }

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

        DateTime oldDateTime = ScheduledDateTime;
        string oldVenue = Venue ?? string.Empty;
        
        ScheduledDateTime = newDateTime;
        
        if (!string.IsNullOrWhiteSpace(newVenue))
            Venue = newVenue;

        Status = FloorballMatchStatus.Scheduled;
        
        // Add domain event
        AddDomainEvent(new FloorballMatchRescheduledEvent(Id, oldDateTime, newDateTime, oldVenue, Venue ?? string.Empty));
    }

    /// <summary>
    /// Postpones the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow postponing</exception>
    public void Postpone()
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot postpone a match with status {Status}.");

        FloorballMatchStatus oldStatus = Status;
        Status = FloorballMatchStatus.Postponed;
        
        // Add domain event
        AddDomainEvent(new FloorballMatchStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Starts the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow starting</exception>
    public void Start()
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start a match with status {Status}.");
        
        if (_officials.Count == 0)
            throw new InvalidOperationException("Cannot start a match without officials.");

        FloorballMatchStatus oldStatus = Status;
        Status = FloorballMatchStatus.InProgress;
        
        // Add domain events
        AddDomainEvent(new FloorballMatchStatusChangedEvent(Id, oldStatus, Status));
        AddDomainEvent(new FloorballMatchStartedEvent(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Records a goal
    /// </summary>
    /// <param name="scoringTeam">The team that scored</param>
    /// <param name="scoringPlayer">The player who scored</param>
    /// <param name="assistingPlayer">The player who assisted (optional)</param>
    /// <param name="periodNumber">The period number (1-3)</param>
    /// <param name="timeInSeconds">The time in seconds when the goal was scored</param>
    /// <param name="description">The description of the goal</param>
    /// <param name="goalType">The type of goal</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public void RecordGoal(
        FloorballTeam scoringTeam, 
        FloorballPlayer scoringPlayer,
        FloorballPlayer assistingPlayer,
        int periodNumber,
        int timeInSeconds,
        string? description = null,
        int? goalType = null)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot record a goal when match status is {Status}.");
        
        ArgumentNullException.ThrowIfNull(scoringTeam);
        ArgumentNullException.ThrowIfNull(scoringPlayer);
        
        if (periodNumber < 1 || periodNumber > 5) // Regular periods (1-3), Overtime (4), Shootout (5)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be between 1 and 5.");
        
        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be non-negative.");
        
        // Check if the scoring team is part of this match
        if (scoringTeam.Id != HomeTeamId && scoringTeam.Id != AwayTeamId)
            throw new ArgumentException("Scoring team is not participating in this match.", nameof(scoringTeam));

        // Check if the scoring player is on the scoring team's roster
        bool playerOnTeam = scoringTeam.Roster.Any(tp => tp.PlayerId == scoringPlayer.Id);
        if (!playerOnTeam)
            throw new ArgumentException("Scoring player is not on the scoring team's roster.", nameof(scoringPlayer));

        // Check if the assisting player is on the scoring team's roster
        if (assistingPlayer != null)
        {
            bool assistingPlayerOnTeam = scoringTeam.Roster.Any(tp => tp.PlayerId == assistingPlayer.Id);
            if (!assistingPlayerOnTeam)
                throw new ArgumentException("Assisting player is not on the scoring team's roster.", nameof(assistingPlayer));
        }

        // Record the goal event
        var goalEvent = new FloorballGoalEvent(
            Id,
            scoringTeam.Id,
            scoringPlayer.Id,
            assistingPlayer?.Id,
            periodNumber,
            timeInSeconds,
            goalType,
            description);

        _events.Add(goalEvent);

        // Update the score
        if (scoringTeam.Id == HomeTeamId)
        {
            HomeScore++;
            // Update the period score for the current period
            FloorballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == periodNumber);
            if (periodScore != null)
            {
                periodScore.IncrementHomeScore();
            }
        }
        else
        {
            AwayScore++;
            // Update the period score for the current period
            FloorballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == periodNumber);
            if (periodScore != null)
            {
                periodScore.IncrementAwayScore();
            }
        }
        
        // Add domain event
        AddDomainEvent(new FloorballGoalScoredEvent(
            Id,
            scoringTeam.Id,
            scoringPlayer.Id,
            periodNumber,
            timeInSeconds,
            WentToOvertime,
            false, // Is shootout goal
            assistingPlayer?.Id));
    }

    /// <summary>
    /// Records a penalty
    /// </summary>
    /// <param name="team">The team that received the penalty</param>
    /// <param name="player">The player who received the penalty</param>
    /// <param name="penaltyType">The type of penalty</param>
    /// <param name="minutes">The duration of the penalty in minutes</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds when the penalty was given</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public void RecordPenalty(
        FloorballTeam team,
        FloorballPlayer player,
        FloorballPenaltyType penaltyType,
        int minutes,
        int periodNumber,
        int timeInSeconds,
        string description = "")
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot record a penalty for a match with status {Status}.");
        if (periodNumber < 1 || periodNumber > _periodScores.Count)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), $"Period number must be between 1 and {_periodScores.Count}.");
        if (timeInSeconds < 0 || timeInSeconds > 1200)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be between 0 and 1200 seconds.");
        if (minutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(minutes), "Penalty minutes must be positive.");
        if(team == null)
            throw new ArgumentNullException(nameof(team), "Team cannot be null.");

        var penaltyEvent = new FloorballPenaltyEvent(
            Id,
            team.Id,
            player?.Id,
            penaltyType,
            minutes,
            periodNumber,
            timeInSeconds,
            description ?? string.Empty);
        _events.Add(penaltyEvent);
        
        // Add domain event
        AddDomainEvent(new FloorballPenaltyAssignedEvent(
            Id,
            team.Id,
            player?.Id,
            penaltyType,
            minutes,
            periodNumber,
            timeInSeconds,
            description ?? string.Empty));
    }

    /// <summary>
    /// Adds an official (referee) to the match
    /// </summary>
    /// <param name="referee">The referee to add</param>
    /// <exception cref="ArgumentNullException">Thrown when the referee is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in a state that allows adding officials</exception>
    public void AddOfficial(FloorballReferee referee)
    {
        ArgumentNullException.ThrowIfNull(referee);
        
        if (Status != FloorballMatchStatus.Scheduled && Status != FloorballMatchStatus.Postponed)
            throw new InvalidOperationException($"Cannot add officials to a match with status {Status}.");
        
        if (_officials.Contains(referee))
            return;
            
        _officials.Add(referee);
        
        // Add domain event
        AddDomainEvent(new FloorballOfficialAssignedEvent(Id, referee.Id));
    }

    /// <summary>
    /// Records that the match went to overtime
    /// </summary>
    public void RecordOvertime()
    {
        WentToOvertime = true;
        
        // Add domain event
        AddDomainEvent(new FloorballMatchOvertimeStartedEvent(Id));
    }

    /// <summary>
    /// Records that the match went to shootout
    /// </summary>
    public void RecordShootout()
    {
        WentToShootout = true;
        
        // Add domain event
        AddDomainEvent(new FloorballMatchShootoutStartedEvent(Id));
    }

    /// <summary>
    /// Completes the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public void Complete()
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a match with status {Status}.");
        
        FloorballMatchStatus oldStatus = Status;
        Status = FloorballMatchStatus.Completed;
        
        // Record that the match has been officiated by all referees
        foreach (FloorballReferee referee in _officials)
        {
            referee.RecordMatchOfficiated();
        }
        
        // Add domain events
        AddDomainEvent(new FloorballMatchStatusChangedEvent(Id, oldStatus, Status));
        AddDomainEvent(new FloorballMatchCompletedEvent(Id, HomeScore, AwayScore, WentToOvertime, WentToShootout));
    }

    /// <summary>
    /// Cancels the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match is already completed</exception>
    public void Cancel()
    {
        if (Status == FloorballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed match.");
            
        FloorballMatchStatus oldStatus = Status;
        Status = FloorballMatchStatus.Cancelled;
        
        // Add domain event
        AddDomainEvent(new FloorballMatchStatusChangedEvent(Id, oldStatus, Status));
    }
} 
