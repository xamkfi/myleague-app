using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches.Events;
using Domain.Enums.Hockey.Matches;
using Domain.Enums.Hockey.Teams;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Matches;

/// <summary>
/// Hockey match aggregate. Home/away truth lives in <see cref="MatchTeams"/>
/// (<see cref="HockeyTeamSlot"/>), not as separate HomeTeamId / AwayTeamId columns.
/// A match may be a standalone scrimmage (<see cref="CompetitionId"/> null) or belong
/// to an optional competition, division, tournament group and/or playoff series.
/// </summary>
public class HockeyMatch : BaseEntity
{
    public Guid? CompetitionId { get; private set; }
    public HockeyCompetition? Competition { get; private set; }

    public Guid? CompetitionDivisionId { get; private set; }
    public HockeyCompetitionDivision? CompetitionDivision { get; private set; }

    public Guid? TournamentGroupId { get; private set; }
    public HockeyTournamentGroup? TournamentGroup { get; private set; }

    public Guid? PlayoffSeriesId { get; private set; }
    public HockeyPlayoffSeries? PlayoffSeries { get; private set; }

    public DateTime ScheduledStartTime { get; private set; }
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? ActualEndTime { get; private set; }
    public string? Venue { get; private set; }

    public HockeyMatchType MatchType { get; private set; }
    public HockeyMatchStatus Status { get; private set; }
    public HockeyMatchResultType? ResultType { get; private set; }
    public HockeyMatchRules MatchRules { get; private set; } = null!;

    public bool CountsTowardStandings { get; private set; }
    public bool CountsTowardPlayerStatistics { get; private set; }
    public bool CountsTowardTeamStatistics { get; private set; }
    public bool CountsTowardGoalieStatistics { get; private set; }
    public bool UsesLineManagement { get; private set; }

    public int CurrentPeriodNumber { get; private set; }
    public bool WentToOvertime { get; private set; }
    public bool WentToShootout { get; private set; }

    public IReadOnlyCollection<HockeyMatchTeam> MatchTeams => _matchTeams.AsReadOnly();
    private readonly List<HockeyMatchTeam> _matchTeams = new();

    public IReadOnlyCollection<HockeyMatchOfficial> Officials => _officials.AsReadOnly();
    private readonly List<HockeyMatchOfficial> _officials = new();

    public IReadOnlyCollection<HockeyPeriodScore> PeriodScores => _periodScores.AsReadOnly();
    private readonly List<HockeyPeriodScore> _periodScores = new();

    public IReadOnlyCollection<HockeyMatchEvent> Events => _events.AsReadOnly();
    private readonly List<HockeyMatchEvent> _events = new();

    /// <summary>Gets the home side match-team, if assigned.</summary>
    public HockeyMatchTeam? HomeMatchTeam =>
        _matchTeams.FirstOrDefault(t => t.TeamSlot == HockeyTeamSlot.Home);

    /// <summary>Gets the away side match-team, if assigned.</summary>
    public HockeyMatchTeam? AwayMatchTeam =>
        _matchTeams.FirstOrDefault(t => t.TeamSlot == HockeyTeamSlot.Away);

    /// <summary>Computed home team id from <see cref="HomeMatchTeam"/>.</summary>
    public Guid? HomeTeamId => HomeMatchTeam?.TeamId;

    /// <summary>Computed away team id from <see cref="AwayMatchTeam"/>.</summary>
    public Guid? AwayTeamId => AwayMatchTeam?.TeamId;

    /// <summary>Computed home score from <see cref="HomeMatchTeam"/> goals (0 if unassigned).</summary>
    public int HomeScore => HomeMatchTeam?.Goals ?? 0;

    /// <summary>Computed away score from <see cref="AwayMatchTeam"/> goals (0 if unassigned).</summary>
    public int AwayScore => AwayMatchTeam?.Goals ?? 0;

    private HockeyMatch() { }

    public HockeyMatch(
        DateTime scheduledStartTime,
        HockeyMatchType matchType,
        HockeyMatchRules? matchRules = null,
        Guid? competitionId = null,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null,
        string? venue = null,
        bool countsTowardStandings = true,
        bool countsTowardPlayerStatistics = true,
        bool countsTowardTeamStatistics = true,
        bool countsTowardGoalieStatistics = true,
        bool usesLineManagement = false)
    {
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));
        if (competitionDivisionId == Guid.Empty)
            throw new ArgumentException("Competition division id cannot be empty.", nameof(competitionDivisionId));
        if (tournamentGroupId == Guid.Empty)
            throw new ArgumentException("Tournament group id cannot be empty.", nameof(tournamentGroupId));
        if (playoffSeriesId == Guid.Empty)
            throw new ArgumentException("Playoff series id cannot be empty.", nameof(playoffSeriesId));

        if (competitionId is null)
        {
            if (competitionDivisionId is not null || tournamentGroupId is not null || playoffSeriesId is not null)
                throw new InvalidOperationException("Division, tournament group and playoff series require a competition.");
        }

        ScheduledStartTime = scheduledStartTime;
        MatchType = matchType;
        MatchRules = matchRules ?? HockeyMatchRules.Default();
        CompetitionId = competitionId;
        CompetitionDivisionId = competitionDivisionId;
        TournamentGroupId = tournamentGroupId;
        PlayoffSeriesId = playoffSeriesId;
        Venue = venue;
        CountsTowardStandings = countsTowardStandings;
        CountsTowardPlayerStatistics = countsTowardPlayerStatistics;
        CountsTowardTeamStatistics = countsTowardTeamStatistics;
        CountsTowardGoalieStatistics = countsTowardGoalieStatistics;
        UsesLineManagement = usesLineManagement;
        Status = HockeyMatchStatus.Scheduled;
        CurrentPeriodNumber = 0;
    }

    /// <summary>
    /// Assigns home or away side. At most one team per slot.
    /// When the match has a <see cref="CompetitionId"/>, <paramref name="competitionTeam"/> is required
    /// and must belong to that competition and the given <paramref name="teamId"/>.
    /// For standalone matches, competition team must not be provided.
    /// </summary>
    public HockeyMatchTeam AssignMatchTeam(
        Guid teamId,
        HockeyTeamSlot slot,
        HockeyCompetitionTeam? competitionTeam = null,
        bool tracksOnIcePlayers = false)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (slot is not (HockeyTeamSlot.Home or HockeyTeamSlot.Away))
            throw new ArgumentException("Match team slot must be Home or Away.", nameof(slot));

        if (_matchTeams.Any(t => t.TeamSlot == slot))
            throw new InvalidOperationException($"A {slot} team is already assigned to this match.");

        if (_matchTeams.Any(t => t.TeamId == teamId))
            throw new InvalidOperationException("Team is already assigned to the other side of this match.");

        Guid? competitionTeamId = null;
        if (CompetitionId is Guid competitionId)
        {
            ArgumentNullException.ThrowIfNull(competitionTeam);
            if (competitionTeam.CompetitionId != competitionId)
                throw new InvalidOperationException("Competition team must belong to the same competition as the match.");
            if (competitionTeam.TeamId != teamId)
                throw new InvalidOperationException("Competition team must reference the same team id.");
            if (!competitionTeam.IsActive)
                throw new InvalidOperationException("Competition team is not active.");

            competitionTeamId = competitionTeam.Id;
        }
        else if (competitionTeam is not null)
        {
            throw new InvalidOperationException("Standalone matches cannot reference a competition team.");
        }

        HockeyMatchTeam matchTeam = new(Id, teamId, slot, competitionTeamId, tracksOnIcePlayers);
        _matchTeams.Add(matchTeam);
        return matchTeam;
    }

    public HockeyMatchOfficial AddOfficial(Guid officialId, HockeyOfficialRole role, bool isMainOfficial = false)
    {
        if (officialId == Guid.Empty)
            throw new ArgumentException("Official id cannot be empty.", nameof(officialId));
        if (_officials.Any(o => o.OfficialId == officialId))
            throw new InvalidOperationException("Official is already assigned to this match.");

        HockeyMatchOfficial matchOfficial = new(Id, officialId, role, isMainOfficial);
        _officials.Add(matchOfficial);
        return matchOfficial;
    }

    public HockeyPeriodScore AddPeriodScore(int periodNumber, HockeyPeriodType periodType)
    {
        HockeyMatchTeam home = HomeMatchTeam
            ?? throw new InvalidOperationException("Home team must be assigned before adding period scores.");
        HockeyMatchTeam away = AwayMatchTeam
            ?? throw new InvalidOperationException("Away team must be assigned before adding period scores.");

        if (_periodScores.Any(p => p.PeriodNumber == periodNumber))
            throw new InvalidOperationException($"Period score for period {periodNumber} already exists.");

        HockeyPeriodScore periodScore = new(Id, periodNumber, periodType, home.Id, away.Id);
        _periodScores.Add(periodScore);
        return periodScore;
    }

    public void UpdateVenue(string? venue) => Venue = venue;

    public void UpdateScheduledStartTime(DateTime scheduledStartTime) => ScheduledStartTime = scheduledStartTime;

    public void SetStatus(HockeyMatchStatus status) => Status = status;

    public void SetResultType(HockeyMatchResultType? resultType) => ResultType = resultType;

    public void MarkStarted(DateTime? actualStartTime = null)
    {
        ActualStartTime = actualStartTime ?? DateTime.UtcNow;
        if (Status == HockeyMatchStatus.Scheduled || Status == HockeyMatchStatus.Warmup)
            Status = HockeyMatchStatus.InProgress;
        if (CurrentPeriodNumber < 1)
            CurrentPeriodNumber = 1;
    }

    public void MarkFinished(DateTime? actualEndTime = null, HockeyMatchResultType? resultType = null)
    {
        ActualEndTime = actualEndTime ?? DateTime.UtcNow;
        Status = HockeyMatchStatus.Finished;
        if (resultType is not null)
            ResultType = resultType;
    }

    public void SetCurrentPeriodNumber(int periodNumber)
    {
        if (periodNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number cannot be negative.");
        CurrentPeriodNumber = periodNumber;
    }

    public void SetWentToOvertime(bool wentToOvertime) => WentToOvertime = wentToOvertime;

    public void SetWentToShootout(bool wentToShootout) => WentToShootout = wentToShootout;

    public void SetTeamGoals(HockeyTeamSlot slot, int goals)
    {
        HockeyMatchTeam matchTeam = GetRequiredMatchTeam(slot);
        matchTeam.SetGoals(goals);
    }

    public void AddEvent(HockeyMatchEvent matchEvent)
    {
        ArgumentNullException.ThrowIfNull(matchEvent);
        if (matchEvent.MatchId != Id)
            throw new InvalidOperationException("Event must belong to this match.");
        if (matchEvent.MatchTeamId is Guid matchTeamId && !_matchTeams.Any(t => t.Id == matchTeamId))
            throw new InvalidOperationException("Event match team must belong to this match.");
        if (_events.Any(e => e.Id == matchEvent.Id))
            throw new InvalidOperationException("Event is already part of this match.");

        _events.Add(matchEvent);

        if (matchEvent is HockeyGoal goal)
        {
            HockeyMatchTeam scoringTeam = _matchTeams.FirstOrDefault(t => t.Id == goal.ScoringMatchTeamId)
                ?? throw new InvalidOperationException("Scoring match team must belong to this match.");
            scoringTeam.IncrementGoals();
        }
    }

    /// <summary>
    /// Removes a match event for live-ops undo. Reverses goal scoreboard side effects.
    /// Competition/season statistics are not adjusted here — recalculate from remaining events.
    /// </summary>
    public HockeyMatchEvent RemoveEvent(Guid eventId)
    {
        EnsureCanRemoveEvents();

        HockeyMatchEvent matchEvent = _events.FirstOrDefault(e => e.Id == eventId)
            ?? throw new ArgumentException($"Event with id '{eventId}' was not found on this match.", nameof(eventId));

        if (matchEvent is HockeyGoal goal)
        {
            HockeyMatchTeam scoringTeam = _matchTeams.FirstOrDefault(t => t.Id == goal.ScoringMatchTeamId)
                ?? throw new InvalidOperationException("Scoring match team must belong to this match.");
            scoringTeam.DecrementGoals();
        }

        _events.Remove(matchEvent);
        return matchEvent;
    }

    /// <summary>
    /// Removes a goal event. Equivalent to <see cref="RemoveEvent"/> with a goal type check.
    /// </summary>
    public HockeyGoal DeleteGoalEvent(Guid goalEventId) =>
        DeleteEventOfType<HockeyGoal>(goalEventId, "Goal");

    /// <summary>
    /// Removes a penalty event.
    /// </summary>
    public HockeyPenalty DeletePenaltyEvent(Guid penaltyEventId) =>
        DeleteEventOfType<HockeyPenalty>(penaltyEventId, "Penalty");

    /// <summary>
    /// Removes a shot event.
    /// </summary>
    public HockeyShot DeleteShotEvent(Guid shotEventId) =>
        DeleteEventOfType<HockeyShot>(shotEventId, "Shot");

    private T DeleteEventOfType<T>(Guid eventId, string eventLabel) where T : HockeyMatchEvent
    {
        EnsureCanRemoveEvents();

        T? matchEvent = _events.OfType<T>().FirstOrDefault(e => e.Id == eventId);
        if (matchEvent is null)
            throw new ArgumentException($"{eventLabel} event with id '{eventId}' was not found on this match.", nameof(eventId));

        if (matchEvent is HockeyGoal goal)
        {
            HockeyMatchTeam scoringTeam = _matchTeams.FirstOrDefault(t => t.Id == goal.ScoringMatchTeamId)
                ?? throw new InvalidOperationException("Scoring match team must belong to this match.");
            scoringTeam.DecrementGoals();
        }

        _events.Remove(matchEvent);
        return matchEvent;
    }

    private void EnsureCanRemoveEvents()
    {
        if (Status is HockeyMatchStatus.Finished
            or HockeyMatchStatus.Cancelled
            or HockeyMatchStatus.Postponed
            or HockeyMatchStatus.Forfeit)
        {
            throw new InvalidOperationException(
                $"Cannot delete match events when the match status is {Status}.");
        }
    }

    /// <summary>
    /// Creates and records a failed coach-challenge penalty when rules require it,
    /// and links it to the given video review.
    /// </summary>
    public HockeyPenalty RecordFailedCoachChallengePenalty(
        HockeyVideoReview review,
        HockeyCoachChallengeRules rules,
        Guid penaltyMatchTeamId)
    {
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(rules);
        if (review.MatchId != Id)
            throw new InvalidOperationException("Video review must belong to this match.");
        if (!_events.Any(e => e.Id == review.Id))
            throw new InvalidOperationException("Video review must be recorded on this match before creating a penalty.");
        if (!_matchTeams.Any(t => t.Id == penaltyMatchTeamId))
            throw new InvalidOperationException("Penalty match team must belong to this match.");

        HockeyPenalty penalty = review.CreateAndLinkFailedChallengePenalty(rules, penaltyMatchTeamId);
        AddEvent(penalty);
        return penalty;
    }

    /// <summary>
    /// Returns true when any match side references the given competition-team id.
    /// Used by the competition aggregate to block removal of still-referenced teams.
    /// </summary>
    public bool ReferencesCompetitionTeam(Guid competitionTeamId) =>
        _matchTeams.Any(t => t.CompetitionTeamId == competitionTeamId);

    private HockeyMatchTeam GetRequiredMatchTeam(HockeyTeamSlot slot) =>
        _matchTeams.FirstOrDefault(t => t.TeamSlot == slot)
        ?? throw new InvalidOperationException($"No {slot} team is assigned to this match.");
}
