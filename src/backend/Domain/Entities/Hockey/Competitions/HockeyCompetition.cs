using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Competitions;
using Domain.ValueObjects.Hockey.Matches;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Competitions;

/// <summary>
/// Abstract base for hockey competitions (seasons, tournaments, etc.).
/// </summary>
public abstract class HockeyCompetition : BaseEntity
{
    public string Name { get; protected set; } = string.Empty;
    public DateTime StartDate { get; protected set; }
    public DateTime EndDate { get; protected set; }
    public HockeyCompetitionType CompetitionType { get; protected set; }
    public HockeyCompetitionStatus Status { get; protected set; }
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
        CompetitionRules = HockeyCompetitionRules.Default();
    }

    protected HockeyCompetition(
        HockeyCompetitionType competitionType,
        string name,
        DateTime startDate,
        DateTime endDate,
        HockeyCompetitionRules? competitionRules = null)
    {
        ValidateName(name);
        ValidateDateRange(startDate, endDate);

        CompetitionType = competitionType;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Status = HockeyCompetitionStatus.Draft;
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

    public void RemoveTeam(Guid teamId)
    {
        EnsureMutable();
        HockeyCompetitionTeam? competitionTeam = GetCompetitionTeam(teamId)
            ?? throw new InvalidOperationException("Team is not participating in this competition.");

        if (_matches.Any(m => m.ReferencesCompetitionTeam(competitionTeam.Id)))
            throw new InvalidOperationException("Cannot remove a team that is part of scheduled matches.");

        competitionTeam.Leave();
    }

    public HockeyCompetitionTeam? GetCompetitionTeam(Guid teamId) =>
        _teams.FirstOrDefault(t => t.TeamId == teamId && t.IsActive);

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
        if (match.HomeCompetitionTeamId is Guid homeId && !IsActiveCompetitionTeam(homeId))
            throw new InvalidOperationException("Home competition team must be participating in this competition.");
        if (match.AwayCompetitionTeamId is Guid awayId && !IsActiveCompetitionTeam(awayId))
            throw new InvalidOperationException("Away competition team must be participating in this competition.");
    }

    private bool IsActiveCompetitionTeam(Guid competitionTeamId) =>
        _teams.Any(t => t.Id == competitionTeamId && t.IsActive);

    private void EnsureMutable()
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
