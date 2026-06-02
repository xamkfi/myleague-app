using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Abstract base class for floorball competitions (seasons, tournaments, etc.)
/// </summary>
public abstract class FloorballCompetition : BaseEntity
{
    /// <summary>
    /// Gets the name of the competition
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets the start date of the competition
    /// </summary>
    public DateTime StartDate { get; protected set; }

    /// <summary>
    /// Gets the end date of the competition
    /// </summary>
    public DateTime EndDate { get; protected set; }

    /// <summary>
    /// Gets whether the competition is currently active
    /// </summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets whether the competition is completed
    /// </summary>
    public bool IsCompleted { get; protected set; }

    /// <summary>
    /// Gets the teams participating in this competition
    /// </summary>
    public IReadOnlyCollection<FloorballTeam> Teams => _teams.AsReadOnly();
    private protected readonly List<FloorballTeam> _teams = new();

    /// <summary>
    /// Gets the matches scheduled for this competition
    /// </summary>
    public IReadOnlyCollection<FloorballMatch> Matches => _matches.AsReadOnly();
    private protected readonly List<FloorballMatch> _matches = new();

    /// <summary>
    /// Gets the match rules configuration for this competition.
    /// Determines period count, duration, overtime and shootout rules.
    /// </summary>
    public FloorballMatchRules MatchRules { get; protected set; }

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected FloorballCompetition()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        StartDate = default;
        EndDate = default;
        IsActive = false;
        IsCompleted = false;
        MatchRules = FloorballMatchRules.Default();
        _teams = new List<FloorballTeam>();
        _matches = new List<FloorballMatch>();
    }

    /// <summary>
    /// Initializes a new instance of the FloorballCompetition class
    /// </summary>
    protected FloorballCompetition(string name, DateTime startDate, DateTime endDate, FloorballMatchRules? matchRules = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Competition name cannot be null or empty.", nameof(name));
        // Same-day competitions are allowed (one-day tournaments are common); only reject when
        // the range is inverted.
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        Id = Guid.NewGuid();
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = false;
        IsCompleted = false;
        MatchRules = matchRules ?? FloorballMatchRules.Default();
        _teams = new List<FloorballTeam>();
        _matches = new List<FloorballMatch>();
    }

    /// <summary>
    /// Updates the competition's details including name and date range
    /// </summary>
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

    /// <summary>
    /// Updates the competition's date range
    /// </summary>
    public void UpdateDateRange(DateTime startDate, DateTime endDate)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update a completed competition.");
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Updates the match rules configuration for this competition.
    /// </summary>
    public void UpdateMatchRules(FloorballMatchRules matchRules)
    {
        ArgumentNullException.ThrowIfNull(matchRules);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update match rules for a completed competition.");

        MatchRules = matchRules;
    }

    /// <summary>
    /// Activates the competition
    /// </summary>
    public void Activate()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot activate a completed competition.");

        IsActive = true;
    }

    /// <summary>
    /// Deactivates the competition
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Completes the competition
    /// </summary>
    public void Complete()
    {
        IsActive = false;
        IsCompleted = true;
    }

    /// <summary>
    /// Adds a team to the competition
    /// </summary>
    public void AddTeam(FloorballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add a team to a completed competition.");
        if (_teams.Contains(team))
            return;
        _teams.Add(team);
    }

    /// <summary>
    /// Removes a team from the competition
    /// </summary>
    public void RemoveTeam(FloorballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot remove a team from a completed competition.");
        if (_matches.Count > 0 && _matches.Any(m => m.HomeTeam == team || m.AwayTeam == team))
            throw new InvalidOperationException("Cannot remove team that is part of scheduled matches.");
        _teams.Remove(team);
    }

    /// <summary>
    /// Adds a match to the competition
    /// </summary>
    public void AddMatch(FloorballMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add a match to a completed competition.");
        if (!_teams.Contains(match.HomeTeam) || !_teams.Contains(match.AwayTeam))
            throw new InvalidOperationException("Both teams must be participating in this competition.");
        if (_matches.Contains(match))
            return;
        _matches.Add(match);
    }
}
