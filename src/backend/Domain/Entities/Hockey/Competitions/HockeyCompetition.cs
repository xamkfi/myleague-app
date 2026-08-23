using Domain.Entities.Hockey.Matches;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Matches;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Abstract base aggregate for hockey competitions (seasons, tournaments, etc.).
/// Owns the competition team chain: teams join via <see cref="AddTeam"/> as
/// <see cref="HockeyCompetitionTeam"/> records, then are placed into divisions,
/// tournament groups or playoff series through that surrogate — never via raw team ids.
/// </summary>
public abstract class HockeyCompetition : BaseEntity
{
    public string Name { get; protected set; } = string.Empty;
    public DateTime StartDate { get; protected set; }
    public DateTime EndDate { get; protected set; }
    public HockeyCompetitionType CompetitionType { get; protected set; }
    public HockeyCompetitionStatus Status { get; protected set; }
    public TeamCategory TeamCategory { get; protected set; }
    public bool IsActive => Status == HockeyCompetitionStatus.Active;
    public bool IsCompleted => Status == HockeyCompetitionStatus.Completed;
    public HockeyCompetitionRules CompetitionRules { get; protected set; } = null!;

    public IReadOnlyCollection<HockeyCompetitionTeam> Teams => _teams.AsReadOnly();
    private protected readonly List<HockeyCompetitionTeam> _teams = new();

    public IReadOnlyCollection<HockeyMatch> Matches => _matches.AsReadOnly();
    private protected readonly List<HockeyMatch> _matches = new();

    public IReadOnlyCollection<HockeyCompetitionDivision> Divisions => _divisions.AsReadOnly();
    private protected readonly List<HockeyCompetitionDivision> _divisions = new();

    public IReadOnlyCollection<HockeyPlayoffSeries> PlayoffSeries => _playoffSeries.AsReadOnly();
    private protected readonly List<HockeyPlayoffSeries> _playoffSeries = new();

    public IReadOnlyCollection<HockeyPlayoffScheduleSlot> PlayoffSchedule => PlayoffScheduleBacking.AsReadOnly();
    private List<HockeyPlayoffScheduleSlot> _playoffSchedule = new();

    private List<HockeyPlayoffScheduleSlot> PlayoffScheduleBacking
    {
        get
        {
            _playoffSchedule ??= new List<HockeyPlayoffScheduleSlot>();
            return _playoffSchedule;
        }
    }

    protected HockeyCompetition()
    {
        Status = HockeyCompetitionStatus.Draft;
        TeamCategory = TeamCategory.Adult;
        CompetitionRules = HockeyCompetitionRules.Default();
    }

    protected HockeyCompetition(
        HockeyCompetitionType competitionType,
        string name,
        DateTime startDate,
        DateTime endDate,
        HockeyCompetitionRules? competitionRules = null,
        TeamCategory teamCategory = TeamCategory.Adult)
    {
        ValidateName(name);
        ValidateDateRange(startDate, endDate);

        CompetitionType = competitionType;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Status = HockeyCompetitionStatus.Draft;
        TeamCategory = teamCategory;
        CompetitionRules = competitionRules ?? HockeyCompetitionRules.Default();
    }

    public void UpdateDetails(string name, DateTime startDate, DateTime endDate)
    {
        EnsureMutable();
        ValidateName(name);
        ValidateDateRange(startDate, endDate);

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void UpdateDateRange(DateTime startDate, DateTime endDate)
    {
        EnsureMutable();
        ValidateDateRange(startDate, endDate);

        StartDate = startDate;
        EndDate = endDate;
    }

    public void UpdateCompetitionRules(HockeyCompetitionRules competitionRules)
    {
        ArgumentNullException.ThrowIfNull(competitionRules);
        EnsureMutable();

        CompetitionRules = competitionRules;
    }

    public void UpdateTeamCategory(TeamCategory teamCategory)
    {
        EnsureMutable();
        TeamCategory = teamCategory;
    }

    public void Publish()
    {
        if (Status != HockeyCompetitionStatus.Draft)
            throw new InvalidOperationException($"Cannot publish a competition in status {Status}.");

        Status = HockeyCompetitionStatus.Published;
    }

    public void OpenRegistration()
    {
        if (Status != HockeyCompetitionStatus.Published)
            throw new InvalidOperationException($"Cannot open registration when status is {Status}.");

        Status = HockeyCompetitionStatus.RegistrationOpen;
    }

    public void Activate()
    {
        if (Status is HockeyCompetitionStatus.Completed or HockeyCompetitionStatus.Cancelled)
            throw new InvalidOperationException($"Cannot activate a competition in status {Status}.");
        if (Status is not (HockeyCompetitionStatus.Published or HockeyCompetitionStatus.RegistrationOpen))
            throw new InvalidOperationException($"Cannot activate a competition in status {Status}.");

        Status = HockeyCompetitionStatus.Active;
    }

    public void Deactivate()
    {
        if (Status != HockeyCompetitionStatus.Active)
            throw new InvalidOperationException($"Cannot deactivate a competition in status {Status}.");

        Status = HockeyCompetitionStatus.Published;
    }

    public void Complete()
    {
        if (Status != HockeyCompetitionStatus.Active)
            throw new InvalidOperationException($"Cannot complete a competition in status {Status}.");

        Status = HockeyCompetitionStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is HockeyCompetitionStatus.Completed or HockeyCompetitionStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel a competition in status {Status}.");

        Status = HockeyCompetitionStatus.Cancelled;
    }

    public HockeyCompetitionTeam AddTeam(Guid teamId, int? seed = null)
    {
        EnsureMutable();
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));

        HockeyCompetitionTeam? existing = GetCompetitionTeam(teamId);
        if (existing is not null)
        {
            if (!existing.IsActive)
                throw new InvalidOperationException("Team has already left this competition.");
            return existing;
        }

        HockeyCompetitionTeam competitionTeam = new(Id, teamId, seed);
        _teams.Add(competitionTeam);
        return competitionTeam;
    }

    /// <summary>
    /// Soft-removes a team from the competition (<see cref="HockeyCompetitionTeam.Leave"/>).
    /// Blocked when the team is referenced by matches, an active division, a tournament
    /// group or a playoff series.
    /// </summary>
    public void RemoveTeam(Guid teamId)
    {
        EnsureMutable();
        HockeyCompetitionTeam? competitionTeam = GetCompetitionTeam(teamId)
            ?? throw new InvalidOperationException("Team is not participating in this competition.");

        if (HasBlockingTeamReferences(competitionTeam))
            throw new InvalidOperationException("Cannot remove a team that is part of scheduled matches, divisions, groups, or playoff series.");

        competitionTeam.Leave();
    }

    /// <summary>Looks up an active competition team by the underlying <c>HockeyTeam</c> id.</summary>
    public HockeyCompetitionTeam? GetCompetitionTeam(Guid teamId) =>
        _teams.FirstOrDefault(t => t.TeamId == teamId && t.IsActive);

    /// <summary>Looks up a competition team by its own surrogate id (active or inactive).</summary>
    public HockeyCompetitionTeam? GetCompetitionTeamById(Guid competitionTeamId) =>
        _teams.FirstOrDefault(t => t.Id == competitionTeamId);

    /// <summary>
    /// Places an active competition team into a division. Validates same-competition
    /// membership and ensures the team is not already in another active division.
    /// </summary>
    public HockeyCompetitionDivisionTeam AddTeamToDivision(Guid competitionDivisionId, Guid competitionTeamId, int? seed = null)
    {
        EnsureMutable();
        ValidateCompetitionTeam(competitionTeamId);

        if (_divisions.Any(d => d.HasActiveTeam(competitionTeamId)))
            throw new InvalidOperationException("Competition team is already assigned to a division.");

        HockeyCompetitionDivision division = _divisions.FirstOrDefault(d => d.Id == competitionDivisionId && d.IsActive)
            ?? throw new InvalidOperationException("Division is not part of this competition.");

        return division.AddTeam(competitionTeamId, seed);
    }

    /// <summary>Soft-removes a competition team from a division.</summary>
    public void RemoveTeamFromDivision(Guid competitionDivisionId, Guid competitionTeamId)
    {
        EnsureMutable();
        HockeyCompetitionDivision division = _divisions.FirstOrDefault(d => d.Id == competitionDivisionId && d.IsActive)
            ?? throw new InvalidOperationException("Division is not part of this competition.");

        division.RemoveTeam(competitionTeamId);
    }

    /// <summary>
    /// Creates and registers a playoff series. Optionally assigns home/away teams,
    /// validating that they are active competition members of this competition.
    /// </summary>
    public HockeyPlayoffSeries CreatePlayoffSeries(
        HockeyPlayoffRound round,
        int seriesOrder,
        int bestOf,
        Guid? homeCompetitionTeamId = null,
        Guid? awayCompetitionTeamId = null)
    {
        EnsureMutable();

        if (homeCompetitionTeamId is Guid homeId)
            ValidateCompetitionTeam(homeId);
        if (awayCompetitionTeamId is Guid awayId)
            ValidateCompetitionTeam(awayId);
        if (homeCompetitionTeamId is Guid home && awayCompetitionTeamId is Guid away && home == away)
            throw new InvalidOperationException("Home and away teams must be different.");

        HockeyPlayoffSeries series = new(Id, round, seriesOrder, bestOf, homeCompetitionTeamId, awayCompetitionTeamId);
        AddPlayoffSeries(series);
        return series;
    }

    /// <summary>
    /// Assigns home and away teams to an existing playoff series.
    /// Both must be active competition members of this competition.
    /// </summary>
    public void AssignPlayoffSeriesTeams(Guid seriesId, Guid homeCompetitionTeamId, Guid awayCompetitionTeamId)
    {
        EnsureMutable();
        ValidateCompetitionTeam(homeCompetitionTeamId);
        ValidateCompetitionTeam(awayCompetitionTeamId);

        HockeyPlayoffSeries series = _playoffSeries.FirstOrDefault(s => s.Id == seriesId)
            ?? throw new InvalidOperationException("Playoff series is not part of this competition.");

        series.AssignTeams(homeCompetitionTeamId, awayCompetitionTeamId);
    }

    public void AddMatch(HockeyMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        EnsureMutable();

        if (match.CompetitionId != Id)
            throw new InvalidOperationException("Match must belong to this competition.");

        ValidateMatchTeams(match);

        if (_matches.Contains(match))
            return;

        _matches.Add(match);
    }

    public void RemoveMatch(HockeyMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        EnsureMutable();

        if (!_matches.Remove(match))
            throw new InvalidOperationException("Match is not part of this competition.");
    }

    public HockeyCompetitionDivision AddDivision(Guid divisionId, string name, int sortOrder, HockeyCompetitionRules? rulesOverride = null)
    {
        EnsureMutable();
        if (_divisions.Any(d => d.DivisionId == divisionId && d.IsActive))
            throw new InvalidOperationException("Division is already part of this competition.");

        HockeyCompetitionDivision division = new(Id, divisionId, name, sortOrder, rulesOverride);
        _divisions.Add(division);
        return division;
    }

    public void RemoveDivision(Guid competitionDivisionId)
    {
        EnsureMutable();
        HockeyCompetitionDivision? division = _divisions.FirstOrDefault(d => d.Id == competitionDivisionId)
            ?? throw new InvalidOperationException("Division is not part of this competition.");

        division.Deactivate();
    }

    public void AddPlayoffSeries(HockeyPlayoffSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);
        EnsureMutable();

        if (series.CompetitionId != Id)
            throw new InvalidOperationException("Playoff series must belong to this competition.");

        if (_playoffSeries.Any(s => s.Round == series.Round && s.SeriesOrder == series.SeriesOrder))
            throw new InvalidOperationException("A playoff series with the same round and order already exists.");

        _playoffSeries.Add(series);
    }

    public void SetPlayoffSchedule(IEnumerable<HockeyPlayoffScheduleSlot> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        EnsureMutable();

        List<HockeyPlayoffScheduleSlot> materialized = slots.ToList();
        HashSet<(HockeyPlayoffRound Round, int SeriesOrder, int MatchOrder)> seen = new();
        foreach (HockeyPlayoffScheduleSlot slot in materialized)
        {
            ArgumentNullException.ThrowIfNull(slot);
            if (!seen.Add((slot.Round, slot.SeriesOrder, slot.MatchOrder)))
                throw new InvalidOperationException($"Playoff schedule contains a duplicate slot for {slot.Round} series {slot.SeriesOrder} match {slot.MatchOrder}.");
        }

        List<HockeyPlayoffScheduleSlot> backing = PlayoffScheduleBacking;
        backing.Clear();
        backing.AddRange(materialized);
    }

    public virtual HockeyCompetitionRules GetEffectiveRules() => CompetitionRules;

    private void ValidateMatchTeams(HockeyMatch match)
    {
        foreach (HockeyMatchTeam matchTeam in match.MatchTeams)
        {
            if (matchTeam.CompetitionTeamId is not Guid competitionTeamId)
                throw new InvalidOperationException("Competition match sides must reference a competition team.");

            ValidateCompetitionTeam(competitionTeamId);
        }
    }

    /// <summary>
    /// Ensures a competition team id refers to an active member of this competition.
    /// Called before placing teams into divisions, groups or playoff series.
    /// </summary>
    private protected void ValidateCompetitionTeam(Guid competitionTeamId)
    {
        if (competitionTeamId == Guid.Empty)
            throw new ArgumentException("Competition team id cannot be empty.", nameof(competitionTeamId));

        HockeyCompetitionTeam? competitionTeam = GetCompetitionTeamById(competitionTeamId)
            ?? throw new InvalidOperationException("Competition team must be participating in this competition.");

        if (competitionTeam.CompetitionId != Id)
            throw new InvalidOperationException("Competition team must belong to this competition.");

        if (!competitionTeam.IsActive)
            throw new InvalidOperationException("Competition team is not active.");
    }

    /// <summary>
    /// Checks whether a competition team is referenced by matches, divisions or playoff
    /// series and therefore cannot be removed. Overridden by <see cref="HockeyTournament"/>
    /// to also check tournament group memberships.
    /// </summary>
    private protected virtual bool HasBlockingTeamReferences(HockeyCompetitionTeam competitionTeam)
    {
        if (_matches.Any(m => m.ReferencesCompetitionTeam(competitionTeam.Id)))
            return true;
        if (_divisions.Any(d => d.HasActiveTeam(competitionTeam.Id)))
            return true;
        if (_playoffSeries.Any(s => s.ReferencesCompetitionTeam(competitionTeam.Id)))
            return true;

        return false;
    }

    private protected void EnsureMutable()
    {
        if (Status is HockeyCompetitionStatus.Completed or HockeyCompetitionStatus.Cancelled)
            throw new InvalidOperationException($"Cannot modify a competition in status {Status}.");
    }

    private static void ValidateName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Competition name cannot be null or empty.", nameof(name));
    }

    private static void ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
    }
}
