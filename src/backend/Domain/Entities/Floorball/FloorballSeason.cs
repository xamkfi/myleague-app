using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball season
/// </summary>
public class FloorballSeason : BaseEntity
{
    /// <summary>
    /// Gets the name of the season (e.g., "2023-2024")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the start date of the season
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the end date of the season
    /// </summary>
    public DateTime EndDate { get; private set; }
    
    /// <summary>
    /// Gets whether the season is currently active
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Gets whether the season is completed
    /// </summary>
    public bool IsCompleted { get; private set; }
    
    /// <summary>
    /// Gets the teams participating in this season
    /// </summary>
    public IReadOnlyCollection<FloorballTeam> Teams => _teams.AsReadOnly();
    private readonly List<FloorballTeam> _teams = new();
    
    /// <summary>
    /// Gets the matches scheduled for this season
    /// </summary>
    public IReadOnlyCollection<FloorballMatch> Matches => _matches.AsReadOnly();
    private readonly List<FloorballMatch> _matches = new();

    /// <summary>
    /// Gets the match rules configuration for this season.
    /// Determines period count, duration, overtime and shootout rules.
    /// </summary>
    public FloorballMatchRules MatchRules { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballSeason()
    {
        Id = Guid.NewGuid();
        Name = string.Empty; // Initialize to avoid CS8618
        StartDate = default; // Initialize to avoid CS8618
        EndDate = default; // Initialize to avoid CS8618
        IsActive = false;
        IsCompleted = false;
        MatchRules = FloorballMatchRules.Default();
        _teams = new List<FloorballTeam>();
        _matches = new List<FloorballMatch>();
    }

    /// <summary>
    /// Initializes a new instance of the FloorballSeason class
    /// </summary>
    /// <param name="name">The name of the season</param>
    /// <param name="startDate">The start date of the season</param>
    /// <param name="endDate">The end date of the season</param>
    /// <param name="matchRules">Optional match rules configuration. If null, defaults are used.</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballSeason(string name, DateTime startDate, DateTime endDate, FloorballMatchRules? matchRules = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Season name cannot be null or empty.", nameof(name));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));
        
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
    /// Updates the season's details including name and date range
    /// </summary>
    /// <param name="name">The new name</param>
    /// <param name="startDate">The new start date</param>
    /// <param name="endDate">The new end date</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the season is completed</exception>
    public void UpdateDetails(string name, DateTime startDate, DateTime endDate)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update a completed season.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Season name cannot be null or empty.", nameof(name));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        
    }

    /// <summary>
    /// Updates the season's date range
    /// </summary>
    /// <param name="startDate">The new start date</param>
    /// <param name="endDate">The new end date</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the season is completed</exception>
    public void UpdateDateRange(DateTime startDate, DateTime endDate)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update a completed season.");
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));

        StartDate = startDate;
        EndDate = endDate;
        
    }


    /// <summary>
    /// Updates the match rules configuration for this season.
    /// </summary>
    /// <param name="matchRules">The new match rules</param>
    /// <exception cref="ArgumentNullException">Thrown when matchRules is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the season is completed</exception>
    public void UpdateMatchRules(FloorballMatchRules matchRules)
    {
        ArgumentNullException.ThrowIfNull(matchRules);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update match rules for a completed season.");

        MatchRules = matchRules;
    }

    /// <summary>
    /// Activates the season
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the season is already completed</exception>
    public void Activate()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot activate a completed season.");

        IsActive = true;
    }

    /// <summary>
    /// Deactivates the season
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Completes the season
    /// </summary>
    public void Complete()
    {
        IsActive = false;
        IsCompleted = true;
    }

    /// <summary>
    /// Adds a team to the season
    /// </summary>
    /// <param name="team">The team to add</param>
    /// <exception cref="ArgumentNullException">Thrown when the team is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the season is completed or the team's division doesn't match</exception>
    public void AddTeam(FloorballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add a team to a completed season.");
        if (_teams.Contains(team))
            return;
        _teams.Add(team);
    }

    /// <summary>
    /// Removes a team from the season
    /// </summary>
    /// <param name="team">The team to remove</param>
    /// <exception cref="ArgumentNullException">Thrown when the team is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the season is completed</exception>
    public void RemoveTeam(FloorballTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot remove a team from a completed season.");
        if (_matches.Count > 0 && _matches.Any(m => m.HomeTeam == team || m.AwayTeam == team))
            throw new InvalidOperationException("Cannot remove team that is part of scheduled matches.");
        _teams.Remove(team);
    }

    /// <summary>
    /// Adds a match to the season
    /// </summary>
    /// <param name="match">The match to add</param>
    /// <exception cref="ArgumentNullException">Thrown when the match is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the season is completed</exception>
    public void AddMatch(FloorballMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add a match to a completed season.");
        if (!_teams.Contains(match.HomeTeam) || !_teams.Contains(match.AwayTeam))
            throw new InvalidOperationException("Both teams must be participating in this season.");
        if (_matches.Contains(match))
            return;
        _matches.Add(match);
        
    }
} 
