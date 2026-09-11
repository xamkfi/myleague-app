using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.ValueObjects.Football;

namespace Domain.Entities.Football.Competitions;

/// <summary>
/// Abstract base for football competitions (seasons and tournaments).
/// </summary>
public abstract class FootballCompetition : BaseEntity
{
    public string Name { get; protected set; }
    public DateTime StartDate { get; protected set; }
    public DateTime EndDate { get; protected set; }
    public bool IsActive { get; protected set; }
    public bool IsCompleted { get; protected set; }
    public TeamCategory TeamCategory { get; protected set; }
    public IReadOnlyCollection<FootballTeam> Teams => _teams.AsReadOnly();
    private protected readonly List<FootballTeam> _teams = new();
    public IReadOnlyCollection<FootballMatch> Matches => _matches.AsReadOnly();
    private protected readonly List<FootballMatch> _matches = new();
    public FootballMatchRules MatchRules { get; protected set; }
    public FootballStandingRules StandingRules { get; protected set; }

    protected FootballCompetition()
    {
        Name = string.Empty;
        TeamCategory = TeamCategory.Adult;
        MatchRules = FootballMatchRules.Default();
        StandingRules = FootballStandingRules.Default();
    }

    protected FootballCompetition(
        string name,
        DateTime startDate,
        DateTime endDate,
        FootballMatchRules? matchRules = null,
        FootballStandingRules? standingRules = null,
        TeamCategory teamCategory = TeamCategory.Adult)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Competition name cannot be null or empty.", nameof(name));
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        TeamCategory = teamCategory;
        MatchRules = matchRules ?? FootballMatchRules.Default();
        StandingRules = standingRules ?? FootballStandingRules.Default();
    }

    public void UpdateDetails(string name, DateTime startDate, DateTime endDate)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update a completed competition.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Competition name cannot be null or empty.", nameof(name));
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void UpdateDateRange(DateTime startDate, DateTime endDate)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update a completed competition.");
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        StartDate = startDate;
        EndDate = endDate;
    }

    public void UpdateMatchRules(FootballMatchRules matchRules)
    {
        ArgumentNullException.ThrowIfNull(matchRules);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update match rules for a completed competition.");
        MatchRules = matchRules;
    }

    public void UpdateStandingRules(FootballStandingRules standingRules)
    {
        ArgumentNullException.ThrowIfNull(standingRules);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update standing rules for a completed competition.");
        StandingRules = standingRules;
    }

    public void UpdateTeamCategory(TeamCategory teamCategory)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update team category for a completed competition.");
        TeamCategory = teamCategory;
    }

    public void Activate()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot activate a completed competition.");
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;

    public void Complete()
    {
        IsActive = false;
        IsCompleted = true;
    }

    public void AddTeam(FootballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add a team to a completed competition.");
        if (_teams.Contains(team))
            return;
        _teams.Add(team);
    }

    public void RemoveTeam(FootballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot remove a team from a completed competition.");
        if (_matches.Count > 0 && _matches.Any(m => m.HomeTeam == team || m.AwayTeam == team))
            throw new InvalidOperationException("Cannot remove team that is part of scheduled matches.");
        _teams.Remove(team);
    }

    public void AddMatch(FootballMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add a match to a completed competition.");
        if (match.HomeTeam != null && !_teams.Contains(match.HomeTeam))
            throw new InvalidOperationException("Home team must be participating in this competition.");
        if (match.AwayTeam != null && !_teams.Contains(match.AwayTeam))
            throw new InvalidOperationException("Away team must be participating in this competition.");
        if (_matches.Contains(match))
            return;
        _matches.Add(match);
    }
}
