using Domain.Enums;
using Domain.Enums.Floorball;
using Domain.ValueObjects.Floorball;
using System.Collections.Generic;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball match
/// </summary>
public class FloorballMatch : BaseEntity
{
    /// <summary>
    /// Gets the competition this match belongs to
    /// </summary>
    public FloorballCompetition Competition { get; private set; }

    /// <summary>
    /// Gets the ID of the competition
    /// </summary>
    public Guid CompetitionId { get; private set; }

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
    /// Gets the match rules configuration snapshot, copied from the season at match creation time.
    /// </summary>
    public FloorballMatchRules MatchRules { get; private set; }
    
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
    /// Gets the tournament stage for this match (null for regular season matches)
    /// </summary>
    public FloorballTournamentStage? TournamentStage { get; private set; }

    /// <summary>
    /// Gets the tournament group ID for group-stage matches (null for non-tournament or playoff matches)
    /// </summary>
    public Guid? TournamentGroupId { get; private set; }

    /// <summary>
    /// Gets the playoff round for playoff matches (null for non-playoff matches)
    /// </summary>
    public FloorballPlayoffRound? PlayoffRound { get; private set; }

    /// <summary>
    /// Gets the display order of this match within its playoff round (0-based, deterministic).
    /// E.g. QF1 = 0, QF2 = 1, ... Used to render the bracket in a stable order.
    /// </summary>
    public int? PlayoffMatchOrder { get; private set; }

    /// <summary>
    /// Gets the next match (the match that the winner of this match advances into).
    /// Null for the final and the optional 3rd place match.
    /// </summary>
    public Guid? NextMatchId { get; private set; }

    /// <summary>
    /// Gets the slot in <see cref="NextMatchId"/> the winner of this match should be placed into.
    /// </summary>
    public FloorballPlayoffSlot? NextMatchSlot { get; private set; }

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
        MatchRules = FloorballMatchRules.Default();
        HomeActiveGoalieId = null;
        AwayActiveGoalieId = null;
        _events = new List<FloorballMatchEvent>();
        _officials = new List<FloorballReferee>();
        _periodScores = new List<FloorballPeriodScore>();
        Competition = null!; // EF Core will set this
        HomeTeam = null!;
        AwayTeam = null!;
        Venue = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballMatch class
    /// </summary>
    /// <param name="competition">The competition this match belongs to</param>
    /// <param name="homeTeam">The home team</param>
    /// <param name="awayTeam">The away team</param>
    /// <param name="scheduledDateTime">The scheduled date and time of the match</param>
    /// <param name="venue">The venue where the match will be played</param>
    public FloorballMatch(
        FloorballCompetition competition,
        FloorballTeam homeTeam,
        FloorballTeam awayTeam,
        DateTime scheduledDateTime,
        string? venue)
        : this(Guid.NewGuid(), competition, homeTeam, awayTeam, scheduledDateTime, venue)
    {
    }

    /// <summary>
    /// Initializes a new instance of the FloorballMatch class with a predefined identifier.
    /// </summary>
    /// <param name="id">The identifier that should be used for the match.</param>
    /// <param name="competition">The competition this match belongs to</param>
    /// <param name="homeTeam">The home team</param>
    /// <param name="awayTeam">The away team</param>
    /// <param name="scheduledDateTime">The scheduled date and time of the match</param>
    /// <param name="venue">The venue where the match will be played</param>
    public FloorballMatch(
        Guid id,
        FloorballCompetition competition,
        FloorballTeam homeTeam,
        FloorballTeam awayTeam,
        DateTime scheduledDateTime,
        string? venue)
        : this(id, competition, homeTeam, awayTeam, scheduledDateTime, venue, matchRulesOverride: null)
    {
    }

    /// <summary>
    /// Internal constructor that accepts an explicit match-rules override. Used by playoff bracket
    /// generation where the competition exposes group-stage rules but we need to copy the playoff
    /// match rules instead.
    /// </summary>
    private FloorballMatch(
        Guid id,
        FloorballCompetition competition,
        FloorballTeam homeTeam,
        FloorballTeam awayTeam,
        DateTime scheduledDateTime,
        string? venue,
        FloorballMatchRules? matchRulesOverride)
    {
        ArgumentNullException.ThrowIfNull(competition);
        ArgumentNullException.ThrowIfNull(homeTeam);
        ArgumentNullException.ThrowIfNull(awayTeam);

        if (homeTeam == awayTeam)
            throw new ArgumentException("Home team and away team cannot be the same team.");

        Id = id;
        Competition = competition;
        CompetitionId = competition.Id;
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
        FloorballMatchRules effectiveRules = matchRulesOverride ?? new FloorballMatchRules(
            competition.MatchRules.NumberOfPeriods,
            competition.MatchRules.PeriodDurationMinutes,
            competition.MatchRules.AllowOvertime,
            competition.MatchRules.OvertimeDurationMinutes,
            competition.MatchRules.AllowShootout);
        MatchRules = effectiveRules;
        HomeActiveGoalieId = null;
        AwayActiveGoalieId = null;
        _events = new List<FloorballMatchEvent>();
        _officials = new List<FloorballReferee>();
        _periodScores = new List<FloorballPeriodScore>();
        for (int i = 1; i <= MatchRules.NumberOfPeriods; i++)
        {
            _periodScores.Add(new FloorballPeriodScore(Id, i, homeTeam.Id, awayTeam.Id));
        }
    }

    /// <summary>
    /// Creates a playoff bracket match. The match rules override defaults to the playoff rules
    /// from the tournament rather than the competition's group-stage rules. Bracket metadata
    /// (round, ordering, forward references) is set after construction via <see cref="SetPlayoffInfo"/>.
    /// </summary>
    public static FloorballMatch CreatePlayoffMatch(
        Guid id,
        FloorballCompetition competition,
        FloorballTeam homeTeam,
        FloorballTeam awayTeam,
        DateTime scheduledDateTime,
        string? venue,
        FloorballMatchRules playoffMatchRules)
    {
        ArgumentNullException.ThrowIfNull(playoffMatchRules);
        return new FloorballMatch(id, competition, homeTeam, awayTeam, scheduledDateTime, venue, playoffMatchRules);
    }

    /// <summary>
    /// Changes the competition for this match
    /// </summary>
    /// <param name="competition">The new competition</param>
    public void ChangeCompetition(FloorballCompetition competition)
    {
        ArgumentNullException.ThrowIfNull(competition);
        Competition = competition;
        CompetitionId = competition.Id;
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

        ScheduledDateTime = newDateTime;
        
        if (!string.IsNullOrWhiteSpace(newVenue))
            Venue = newVenue;

        Status = FloorballMatchStatus.Scheduled;
        
    }

    /// <summary>
    /// Postpones the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match status doesn't allow postponing</exception>
    public void Postpone()
    {
        if (Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException($"Cannot postpone a match with status {Status}.");

        Status = FloorballMatchStatus.Postponed;
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

        if (HomeActiveGoalieId is null || AwayActiveGoalieId is null)
        {
            throw new InvalidOperationException("A match cannot start without goalies assigned for both teams.");
        }

        Status = FloorballMatchStatus.InProgress;
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
        EnsureInProgress("record a goal");

        ArgumentNullException.ThrowIfNull(scoringTeam);
        ArgumentNullException.ThrowIfNull(scoringPlayer);

        if (periodNumber < 1 || periodNumber > ShootoutPeriodNumber)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), $"Period number must be between 1 and {ShootoutPeriodNumber}.");

        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be non-negative.");

        if (scoringTeam.Id != HomeTeamId && scoringTeam.Id != AwayTeamId)
            throw new ArgumentException("Scoring team is not participating in this match.", nameof(scoringTeam));

        ValidatePlayerOnRoster(scoringTeam, scoringPlayer.Id, "Scoring player");

        if (assistingPlayer != null)
            ValidatePlayerOnRoster(scoringTeam, assistingPlayer.Id, "Assisting player");

        if (secondaryAssistingPlayer != null)
            ValidatePlayerOnRoster(scoringTeam, secondaryAssistingPlayer.Id, "Secondary assisting player");

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
        AdjustScore(scoringTeam.Id, periodNumber, increment: true);

        return goalEvent;
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
        EnsureInProgress("record a penalty");
        ArgumentNullException.ThrowIfNull(team);

        if (periodNumber < 1 || periodNumber > _periodScores.Count)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), $"Period number must be between 1 and {_periodScores.Count}.");
        if (timeInSeconds < 0 || timeInSeconds > 1200)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be between 0 and 1200 seconds.");
        if (minutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(minutes), "Penalty minutes must be positive.");

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
        EnsureInProgress("record a save");

        ArgumentNullException.ThrowIfNull(team);
        ArgumentNullException.ThrowIfNull(goalie);

        if (periodNumber < 1 || periodNumber > ShootoutPeriodNumber)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), $"Period number must be between 1 and {ShootoutPeriodNumber}.");
        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time must be non-negative.");
        if (team.Id != HomeTeamId && team.Id != AwayTeamId)
            throw new ArgumentException("Team is not participating in this match.", nameof(team));

        ValidatePlayerOnRoster(team, goalie.Id, "Goalie");

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

        if (Status == FloorballMatchStatus.Completed || Status == FloorballMatchStatus.Cancelled)
            throw new InvalidOperationException($"Cannot add officials to a match with status {Status}.");

        if (_officials.Contains(referee))
            return;

        _officials.Add(referee);
    }

    /// <summary>
    /// Removes an official (referee) from the match. Ensures at least one official remains.
    /// </summary>
    /// <param name="refereeId">The referee ID to remove</param>
    /// <exception cref="InvalidOperationException">Thrown when removal would leave zero officials or match status disallows change</exception>
    public void RemoveOfficial(Guid refereeId)
    {
        if (Status == FloorballMatchStatus.Completed || Status == FloorballMatchStatus.Cancelled)
            throw new InvalidOperationException($"Cannot remove officials from a match with status {Status}.");

        FloorballReferee? existing = _officials.FirstOrDefault(o => o.Id == refereeId);
        if (existing == null)
            return;

        if (_officials.Count <= 1)
            throw new InvalidOperationException("Cannot remove the last official from the match.");

        _officials.Remove(existing);
    }

    /// <summary>
    /// Replaces the officials collection with the provided set. Requires at least one official.
    /// </summary>
    public void SetOfficials(IEnumerable<FloorballReferee> officials)
    {
        ArgumentNullException.ThrowIfNull(officials);
        if (Status == FloorballMatchStatus.Completed || Status == FloorballMatchStatus.Cancelled)
            throw new InvalidOperationException($"Cannot update officials when match status is {Status}.");

        List<FloorballReferee> refs = officials.Distinct().ToList();
        if (refs.Count == 0)
            throw new InvalidOperationException("Match must have at least one official.");

        _officials.Clear();
        _officials.AddRange(refs);
    }

    /// <summary>
    /// Gets the period number used for overtime (regular periods + 1).
    /// </summary>
    public int OvertimePeriodNumber => MatchRules.NumberOfPeriods + 1;

    /// <summary>
    /// Gets the period number used for shootout (regular periods + 2).
    /// </summary>
    public int ShootoutPeriodNumber => MatchRules.NumberOfPeriods + 2;

    /// <summary>
    /// Records that the match went to overtime
    /// </summary>
    public void RecordOvertime()
    {
        if (!MatchRules.AllowOvertime)
            throw new InvalidOperationException("Overtime is not allowed by the match rules.");

        WentToOvertime = true;

        int overtimePeriod = OvertimePeriodNumber;
        // Create a periodscore for non-regular period (Overtime)
        if (_periodScores.All(ps => ps.PeriodNumber != overtimePeriod))
        {
            _periodScores.Add(new FloorballPeriodScore(Id, overtimePeriod, HomeTeamId, AwayTeamId));
        }
    }

    /// <summary>
    /// Records that the match went to shootout
    /// </summary>
    public void RecordShootout()
    {
        if (!MatchRules.AllowShootout)
            throw new InvalidOperationException("Shootout is not allowed by the match rules.");

        WentToShootout = true;

        int shootoutPeriod = ShootoutPeriodNumber;
        // Create a periodscore for non-regular period (Shootout)
        if (_periodScores.All(ps => ps.PeriodNumber != shootoutPeriod))
        {
            _periodScores.Add(new FloorballPeriodScore(Id, shootoutPeriod, HomeTeamId, AwayTeamId));
        }
    }

    /// <summary>
    /// Completes the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress</exception>
    public void Complete()
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a match with status {Status}.");

        // Playoff matches must have a winner: overtime/shootout codepaths increment scores on the
        // OT/SO period scores and feed back into HomeScore/AwayScore, so an equal final score here
        // means the tie was never resolved. Bracket advancement relies on a unique winner.
        if (PlayoffRound != null && HomeScore == AwayScore)
            throw new InvalidOperationException("Playoff matches cannot end in a draw. Record overtime or shootout result first.");

        Status = FloorballMatchStatus.Completed;

        // Record that the match has been officiated by all referees
        foreach (FloorballReferee referee in _officials)
        {
            referee.RecordMatchOfficiated();
        }
    }

    /// <summary>
    /// Cancels the match
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match is already completed</exception>
    public void Cancel()
    {
        if (Status == FloorballMatchStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed match.");
            
        Status = FloorballMatchStatus.Cancelled;
    }

    /// <summary>
    /// Reactivates a cancelled match back to Scheduled status
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the match is not cancelled</exception>
    public void Reactivate()
    {
        if (Status != FloorballMatchStatus.Cancelled)
            throw new InvalidOperationException("Can only reactivate a cancelled match.");

        Status = FloorballMatchStatus.Scheduled;
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
        AdjustScore(goalEvent.TeamId, goalEvent.PeriodNumber, increment: false);

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

        return penaltyEvent;
    }

    /// <summary>
    /// Deletes a save event from the match
    /// </summary>
    /// <param name="saveEventId">The ID of the save event to delete</param>
    /// <returns>The deleted save event</returns>
    /// <exception cref="ArgumentException">Thrown when the save event is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in a state that allows deleting saves</exception>
    public FloorballSave DeleteSaveEvent(Guid saveEventId)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException("Cannot delete save events unless match is in progress.");

        // Find the save event
        FloorballSave? saveEvent = _events.OfType<FloorballSave>().FirstOrDefault(s => s.Id == saveEventId);
        if (saveEvent == null)
            throw new ArgumentException($"Save event with ID {saveEventId} not found in this match.", nameof(saveEventId));

        // Remove the save event
        _events.Remove(saveEvent);

        // No direct score changes for saves

        return saveEvent;
    }

    public void EndPeriod(int periodNumber)
    {
        EnsureInProgress("end a period");
        if (periodNumber < 1 || periodNumber > ShootoutPeriodNumber)
            throw new ArgumentOutOfRangeException(nameof(periodNumber));

        FloorballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == periodNumber);

        if(periodScore == null)
            throw new InvalidOperationException($"Period {periodNumber} has not been started.");

        periodScore.Complete();
    }

    /// <summary>
    /// Sets the active goalie for a team participating in this match.
    /// </summary>
    /// <param name="teamId">The ID of the team (must be home or away)</param>
    /// <param name="goalieId">The ID of the goalie to set as active</param>
    /// <exception cref="InvalidOperationException">Thrown when the match is not in progress or scheduled</exception>
    /// <exception cref="ArgumentException">Thrown when the team is not part of this match or the goalie is not on the team</exception>
    public void SetActiveGoalie(Guid teamId, Guid goalieId)
    {
        if (Status != FloorballMatchStatus.InProgress && Status != FloorballMatchStatus.Scheduled)
            throw new InvalidOperationException("Cannot change goalie when match is not in progress or scheduled.");

        if (teamId == HomeTeamId)
        {
            ValidatePlayerOnRoster(HomeTeam, goalieId, "Goalie");
            HomeActiveGoalieId = goalieId;
        }
        else if (teamId == AwayTeamId)
        {
            ValidatePlayerOnRoster(AwayTeam, goalieId, "Goalie");
            AwayActiveGoalieId = goalieId;
        }
        else
        {
            throw new ArgumentException("Team is not participating in this match.", nameof(teamId));
        }
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

    /// <summary>
    /// Sets tournament-specific metadata on this match
    /// </summary>
    public void SetTournamentInfo(FloorballTournamentStage stage, Guid? groupId = null)
    {
        TournamentStage = stage;
        TournamentGroupId = groupId;
    }

    /// <summary>
    /// Sets playoff bracket metadata on this match. Called by the bracket generator when the
    /// playoff stage is started so the read-side can render rounds, ordering and forward references.
    /// </summary>
    public void SetPlayoffInfo(
        FloorballPlayoffRound round,
        int matchOrder,
        Guid? nextMatchId,
        FloorballPlayoffSlot? nextMatchSlot)
    {
        if (matchOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(matchOrder), "Playoff match order must be non-negative.");
        if (nextMatchId.HasValue != nextMatchSlot.HasValue)
            throw new ArgumentException("NextMatchId and NextMatchSlot must be provided together.");

        PlayoffRound = round;
        PlayoffMatchOrder = matchOrder;
        NextMatchId = nextMatchId;
        NextMatchSlot = nextMatchSlot;
        TournamentStage = MapRoundToStage(round);
    }

    /// <summary>
    /// Replaces a team slot on this playoff match. Used when a feeder match completes and the
    /// winner has to be propagated into this (still scheduled) match.
    /// </summary>
    public void AssignPlayoffTeam(FloorballPlayoffSlot slot, FloorballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (Status != FloorballMatchStatus.Scheduled && Status != FloorballMatchStatus.Postponed)
            throw new InvalidOperationException($"Cannot assign a playoff team when match status is {Status}.");

        if (slot == FloorballPlayoffSlot.Home)
        {
            HomeTeam = team;
            HomeTeamId = team.Id;
        }
        else
        {
            AwayTeam = team;
            AwayTeamId = team.Id;
        }
    }

    private static FloorballTournamentStage MapRoundToStage(FloorballPlayoffRound round) =>
        round switch
        {
            FloorballPlayoffRound.QuarterFinal => FloorballTournamentStage.Quarterfinal,
            FloorballPlayoffRound.SemiFinal => FloorballTournamentStage.Semifinal,
            FloorballPlayoffRound.ThirdPlaceMatch => FloorballTournamentStage.ThirdPlace,
            FloorballPlayoffRound.Final => FloorballTournamentStage.Final,
            _ => FloorballTournamentStage.None
        };

    // ── Private helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Guards that the match is currently in progress.
    /// </summary>
    private void EnsureInProgress(string action)
    {
        if (Status != FloorballMatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot {action} when match status is {Status}.");
    }

    /// <summary>
    /// Validates that a player is on the given team's roster.
    /// </summary>
    private static void ValidatePlayerOnRoster(FloorballTeam team, Guid playerId, string roleName)
    {
        if (!team.Roster.Any(tp => tp.PlayerId == playerId))
            throw new ArgumentException($"{roleName} is not on the team's roster.");
    }

    /// <summary>
    /// Adjusts match and period scores when a goal is added or removed.
    /// </summary>
    /// <param name="teamId">Scoring team id</param>
    /// <param name="periodNumber">Period the goal belongs to</param>
    /// <param name="increment">True to add, false to subtract</param>
    private void AdjustScore(Guid teamId, int periodNumber, bool increment)
    {
        bool isHome = teamId == HomeTeamId;

        if (isHome)
        {
            if (increment) HomeScore++; else HomeScore--;
        }
        else
        {
            if (increment) AwayScore++; else AwayScore--;
        }

        FloorballPeriodScore? periodScore = _periodScores.FirstOrDefault(ps => ps.PeriodNumber == periodNumber);
        if (periodScore != null)
        {
            if (isHome)
            {
                if (increment) periodScore.IncrementHomeScore(); else periodScore.DecrementHomeScore();
            }
            else
            {
                if (increment) periodScore.IncrementAwayScore(); else periodScore.DecrementAwayScore();
            }
        }
    }
}
