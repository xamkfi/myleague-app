using Domain.Enums;
using Domain.Enums.Floorball;
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
    public IReadOnlyCollection<FloorballMatchEvent> Events => _events.AsReadOnly();
    private readonly List<FloorballMatchEvent> _events = new();

    /// <summary>
    /// Gets all goal events
    /// </summary>
    public IReadOnlyCollection<FloorballGoal> GoalEvents => 
        _events.OfType<FloorballGoal>().ToList().AsReadOnly();

    /// <summary>
    /// Gets all penalty events
    /// </summary>
    public IReadOnlyCollection<FloorballPenalty> PenaltyEvents => 
        _events.OfType<FloorballPenalty>().ToList().AsReadOnly();
    
    /// <summary>
    /// Gets all save events
    /// </summary>
    public IReadOnlyCollection<FloorballSave> SaveEvents =>
        _events.OfType<FloorballSave>().ToList().AsReadOnly();

    /// <summary>
    /// Gets the ID of the current active goalie for the home team
    /// </summary>
    public Guid? HomeActiveGoalieId { get; private set; }

    /// <summary>
    /// Gets the ID of the current active goalie for the away team
    /// </summary>
    public Guid? AwayActiveGoalieId { get; private set; }
    
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
        HomeActiveGoalieId = null;
        AwayActiveGoalieId = null;
        _events = new List<FloorballMatchEvent>();
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
        HomeActiveGoalieId = null;
        AwayActiveGoalieId = null;
        _events = new List<FloorballMatchEvent>();
        _officials = new List<FloorballReferee>();
        _periodScores = new List<FloorballPeriodScore>();
        for (int i = 1; i <= 3; i++)
        {
            _periodScores.Add(new FloorballPeriodScore(Id, i, homeTeam.Id, awayTeam.Id));
        }
    }

    /// <summary>
    /// Initializes a new instance of the FloorballMatch class with a predefined identifier.
    /// This overload is intended for projections so that the read-model row uses exactly
    /// the same Guid as EventSourcedFloorballMatch aggregateId.
    /// </summary>
    /// <param name="id">The identifier that should be used for the match.</param>
    /// <param name="season">The season this match belongs to</param>
    /// <param name="homeTeam">The home team</param>
    /// <param name="awayTeam">The away team</param>
    /// <param name="scheduledDateTime">The scheduled date and time of the match</param>
    /// <param name="venue">The venue where the match will be played</param>
    /// <exception cref="ArgumentNullException">Thrown when a required parameter is null</exception>
    /// <exception cref="ArgumentException">Thrown when teams are the same or venue is invalid</exception>
    public FloorballMatch(
        Guid id,
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

        Id = id;
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
        HomeActiveGoalieId = null;
        AwayActiveGoalieId = null;
        _events = new List<FloorballMatchEvent>();
        _officials = new List<FloorballReferee>();
        _periodScores = new List<FloorballPeriodScore>();
        for (int i = 1; i <= 3; i++)
        {
            _periodScores.Add(new FloorballPeriodScore(Id, i, homeTeam.Id, awayTeam.Id));
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
    /// Changes the season for this match
    /// </summary>
    /// <param name="season">The new season</param>
    /// <exception cref="ArgumentNullException">Thrown when season is null</exception>
    public void ChangeSeason(FloorballSeason season)
    {
        ArgumentNullException.ThrowIfNull(season);
        Season = season;
        SeasonId = season.Id;
    }

    /// <summary>
    /// Changes the teams for this match
    /// </summary>
    /// <param name="homeTeam">The new home team</param>
    /// <param name="awayTeam">The new away team</param>
    /// <exception cref="ArgumentNullException">Thrown when homeTeam or awayTeam is null</exception>
    public void ChangeTeams(FloorballTeam homeTeam, FloorballTeam awayTeam)
    {
        ArgumentNullException.ThrowIfNull(homeTeam);
        ArgumentNullException.ThrowIfNull(awayTeam);
        HomeTeam = homeTeam;
        HomeTeamId = homeTeam.Id;
        AwayTeam = awayTeam;
        AwayTeamId = awayTeam.Id;
    }

    /// <summary>
    /// Changes the venue for this match
    /// </summary>
    /// <param name="venue">The new venue</param>
    /// <exception cref="ArgumentNullException">Thrown when venue is null</exception>
    public void ChangeVenue(string venue)
    {
        ArgumentNullException.ThrowIfNull(venue);
        Venue = venue;
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

        //Add domain events
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
    public FloorballGoal RecordGoal(
        FloorballTeam scoringTeam, 
        FloorballPlayer scoringPlayer,
        FloorballPlayer? assistingPlayer,
        FloorballPlayer? secondaryAssistingPlayer,
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
        // Check if the assisting player is on the scoring team's roster
        if (secondaryAssistingPlayer != null)
        {
            bool secondaryAssistingPlayerOnTeam = scoringTeam.Roster.Any(tp => tp.PlayerId == secondaryAssistingPlayer.Id);
            if (!secondaryAssistingPlayerOnTeam)
                throw new ArgumentException("Secondary Assisting player is not on the scoring team's roster.", nameof(assistingPlayer));
        }

        // Record the goal event
        FloorballGoal goalEvent = new FloorballGoal(
            matchId: Id,
            scoringTeam.Id,
            scoringPlayer.Id,
            assistingPlayer?.Id,
            secondaryAssistingPlayer?.Id,
            periodNumber,
            timeInSeconds,
            null,
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
            false, // isPenaltyShot
            WentToShootout, // isShootout
            assistingPlayer?.Id,
            secondaryAssistingPlayer?.Id));

        return goalEvent;
    }

    public void UpdateScore(Guid scoringTeamId)
    {
        if (scoringTeamId == HomeTeamId)
            HomeScore++;
        else
            AwayScore++;
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
    public FloorballPenalty RecordPenalty(
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

        FloorballPenalty penaltyEvent = new FloorballPenalty(
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

        return penaltyEvent;
    }

    /// <summary>
    /// Records a save
    /// </summary>
    /// <param name="team">The team whose goalie made the save</param>
    /// <param name="goalie">The goalie who made the save</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds when the save was made</param>
    /// <param name="wasInOvertime">Whether the save was made in overtime</param>
    /// <param name="wasInShootout">Whether the save was made in shootout</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public FloorballSave RecordSave(
        FloorballTeam team,
        FloorballPlayer goalie,
        int periodNumber,
        int timeInSeconds,
        bool wasInOvertime = false,
        bool wasInShootout = false)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot record a save when match status is {Status}.");

        ArgumentNullException.ThrowIfNull(team);
        ArgumentNullException.ThrowIfNull(goalie);

        if (periodNumber < 1 || periodNumber > 5)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be between 1 and 5.");
        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be non-negative.");
        if (team.Id != HomeTeamId && team.Id != AwayTeamId)
            throw new ArgumentException("Team is not participating in this match.", nameof(team));
        bool goalieOnTeam = team.Roster.Any(tp => tp.PlayerId == goalie.Id);
        if (!goalieOnTeam)
            throw new ArgumentException("Goalie is not on the team's roster.", nameof(goalie));

        FloorballSave saveEvent = new FloorballSave(
            Guid.NewGuid(),
            Id,
            team.Id,
            goalie.Id,
            periodNumber,
            timeInSeconds,
            wasInOvertime,
            wasInShootout);
        _events.Add(saveEvent);

        // Add domain event
        AddDomainEvent(new FloorballSaveEvent(
            Id,
            team.Id,
            goalie.Id,
            periodNumber,
            timeInSeconds,
            wasInOvertime,
            wasInShootout));

        return saveEvent;
    }

    /// <summary>
    /// Adds an official (referee) to the match
    /// </summary>
    /// <param name="referee">The referee to add</param>
    /// <param name="addDomainEvent">Whether to add a domain event</param>
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

        //Add domain events
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

    /// <summary>
    /// Deletes a goal event from the match
    /// </summary>
    /// <param name="goalEventId">The ID of the goal event to delete</param>
    /// <returns>The deleted goal event</returns>
    /// <exception cref="ArgumentException">Thrown when the goal event is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in a state that allows deleting goals</exception>
    public FloorballGoal DeleteGoalEvent(Guid goalEventId)
    {
        if (Status == FloorballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot delete goal events from a completed match.");

        // Find the goal event
        FloorballGoal? goalEvent = _events.OfType<FloorballGoal>().FirstOrDefault(g => g.Id == goalEventId);
        if (goalEvent == null)
            throw new ArgumentException($"Goal event with ID {goalEventId} not found in this match.", nameof(goalEventId));

        // Remove the goal event
        _events.Remove(goalEvent);

        // Update the score
        if (goalEvent.TeamId == HomeTeamId)
        {
            HomeScore--;
            // Update the period score for the goal's period
            FloorballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == goalEvent.PeriodNumber);
            if (periodScore != null)
            {
                periodScore.DecrementHomeScore();
            }
        }
        else
        {
            AwayScore--;
            // Update the period score for the goal's period
            FloorballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == goalEvent.PeriodNumber);
            if (periodScore != null)
            {
                periodScore.DecrementAwayScore();
            }
        }

        // Add domain event for goal deletion
        AddDomainEvent(new FloorballGoalDeletedEvent(
            Id,
            goalEvent.TeamId,
            goalEvent.ScoringPlayerId,
            goalEvent.PeriodNumber,
            goalEvent.TimeInSeconds,
            goalEvent.AssistingPlayerId));

        return goalEvent;
    }

    /// <summary>
    /// Deletes a penalty event from the match
    /// </summary>
    /// <param name="penaltyEventId">The ID of the penalty event to delete</param>
    /// <returns>The deleted penalty event</returns>
    /// <exception cref="ArgumentException">Thrown when the penalty event is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in a state that allows deleting penalties</exception>
    public FloorballPenalty DeletePenaltyEvent(Guid penaltyEventId)
    {
        if (Status == FloorballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot delete penalty events from a completed match.");

        // Find the penalty event
        FloorballPenalty? penaltyEvent = _events.OfType<FloorballPenalty>().FirstOrDefault(p => p.Id == penaltyEventId);
        if (penaltyEvent == null)
            throw new ArgumentException($"Penalty event with ID {penaltyEventId} not found in this match.", nameof(penaltyEventId));

        // Remove the penalty event
        _events.Remove(penaltyEvent);

        // Add domain event for penalty deletion
        AddDomainEvent(new FloorballPenaltyDeletedEvent(
            Id,
            penaltyEvent.TeamId,
            penaltyEvent.PlayerId,
            penaltyEvent.PenaltyType,
            penaltyEvent.DurationInMinutes,
            penaltyEvent.PeriodNumber,
            penaltyEvent.TimeInSeconds,
            penaltyEvent.Description));

        return penaltyEvent;
    }

    public void EndPeriod(int periodNumber)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException("Match must be in progress.");
        if (periodNumber < 1 || periodNumber > 5)
            throw new ArgumentOutOfRangeException(nameof(periodNumber));

        FloorballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == periodNumber);

        if(periodScore == null)
            throw new InvalidOperationException($"Period {periodNumber} has not been started.");

        periodScore.Complete();

        AddDomainEvent(new FloorballPeriodEndedEvent(Id, periodNumber, HomeScore, AwayScore, periodNumber == 3));
    }

    /// <summary>
    /// Sets the active goalie for the home team
    /// </summary>
    /// <param name="goalieId">The ID of the goalie to set as active</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    /// <exception cref="ArgumentException">Thrown when the goalie is not on the home team</exception>
    public void SetHomeActiveGoalie(Guid goalieId)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException("Cannot change goalie when match is not in progress.");

        // Validate that the goalie is on the home team
        bool goalieOnTeam = HomeTeam.Roster.Any(tp => tp.PlayerId == goalieId);
        if (!goalieOnTeam)
            throw new ArgumentException("Goalie is not on the home team.", nameof(goalieId));

        Guid? previousGoalieId = HomeActiveGoalieId;
        HomeActiveGoalieId = goalieId;

        // Add domain event
        AddDomainEvent(new FloorballGoalieChangedEvent(Id, HomeTeamId, previousGoalieId, goalieId));
    }

    /// <summary>
    /// Sets the active goalie for the away team
    /// </summary>
    /// <param name="goalieId">The ID of the goalie to set as active</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    /// <exception cref="ArgumentException">Thrown when the goalie is not on the away team</exception>
    public void SetAwayActiveGoalie(Guid goalieId)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException("Cannot change goalie when match is not in progress.");

        // Validate that the goalie is on the away team
        bool goalieOnTeam = AwayTeam.Roster.Any(tp => tp.PlayerId == goalieId);
        if (!goalieOnTeam)
            throw new ArgumentException("Goalie is not on the away team.", nameof(goalieId));

        Guid? previousGoalieId = AwayActiveGoalieId;
        AwayActiveGoalieId = goalieId;

        // Add domain event
        AddDomainEvent(new FloorballGoalieChangedEvent(Id, AwayTeamId, previousGoalieId, goalieId));
    }

    /// <summary>
    /// Gets the active goalie ID for a specific team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>The active goalie ID, or null if no goalie is set</returns>
    public Guid? GetActiveGoalieId(Guid teamId)
    {
        if (teamId == HomeTeamId)
            return HomeActiveGoalieId;
        else if (teamId == AwayTeamId)
            return AwayActiveGoalieId;
        else
            throw new ArgumentException("Team is not participating in this match.", nameof(teamId));
    }
} 
