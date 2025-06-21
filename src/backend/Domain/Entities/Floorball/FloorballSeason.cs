using Domain.EventSourcing;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Common;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball season
/// </summary>
public class FloorballSeason : AggregateRoot
{
    /// <summary>
    /// Gets the name of the season (e.g., "2023-2024")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the division this season belongs to
    /// </summary>
    public Division Division { get; private set; }

    /// <summary>
    /// Gets the ID of the division this season belongs to
    /// </summary>
    public Guid DivisionId { get; private set; }

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
    /// Private constructor for EF Core
    /// </summary>
    private FloorballSeason()
    {
        Id = Guid.NewGuid();
        Name = string.Empty; // Initialize to avoid CS8618
        Division = default!; // Initialize to avoid CS8618
        DivisionId = Guid.Empty; // Initialize to avoid CS8618
        StartDate = default; // Initialize to avoid CS8618
        EndDate = default; // Initialize to avoid CS8618
        IsActive = false;
        IsCompleted = false;
        _teams = new List<FloorballTeam>();
        _matches = new List<FloorballMatch>();
    }

    /// <summary>
    /// Initializes a new instance of the FloorballSeason class
    /// </summary>
    /// <param name="name">The name of the season</param>
    /// <param name="division">The division this season belongs to</param>
    /// <param name="startDate">The start date of the season</param>
    /// <param name="endDate">The end date of the season</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballSeason(string name, Guid divisionId, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Season name cannot be null or empty.", nameof(name));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));
        
        Id = Guid.NewGuid();
        Name = name;
        Division = default!;
        DivisionId = divisionId;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = false;
        IsCompleted = false;
        _teams = new List<FloorballTeam>();
        _matches = new List<FloorballMatch>();
        
        AddDomainEvent(new FloorballSeasonCreatedEvent(Id, name, divisionId, startDate, endDate));
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
        
        AddDomainEvent(new FloorballSeasonDetailsUpdatedEvent(Id, name, startDate, endDate));
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
        
        AddDomainEvent(new FloorballSeasonDateRangeUpdatedEvent(Id, startDate, endDate));
    }

    /// <summary>
    /// Updates the division of the season
    /// </summary>
    /// <param name="division">The new division</param>
    /// <exception cref="InvalidOperationException">Thrown when the season is completed or has teams in a different division</exception>
    public void UpdateDivision(Division division)
    {
        ArgumentNullException.ThrowIfNull(division);
        
        if (IsCompleted)
            throw new InvalidOperationException("Cannot update a completed season.");
        if (_teams.Count > 0 && _teams.Any(t => t.DivisionId != division.Id))
            throw new InvalidOperationException("Cannot change division because some teams in this season belong to a different division.");
        
        Division = division;
        DivisionId = division.Id;
        
        AddDomainEvent(new FloorballSeasonDivisionUpdatedEvent(Id, division));
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
        AddDomainEvent(new FloorballSeasonActivatedEvent(Id));
    }

    /// <summary>
    /// Deactivates the season
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new FloorballSeasonDeactivatedEvent(Id));
    }

    /// <summary>
    /// Completes the season
    /// </summary>
    public void Complete()
    {
        IsActive = false;
        IsCompleted = true;
        AddDomainEvent(new FloorballSeasonCompletedEvent(Id));
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
        if (team.DivisionId != DivisionId)
            throw new InvalidOperationException($"Team division does not match season division.");
        if (_teams.Contains(team))
            return;
        _teams.Add(team);
        AddDomainEvent(new FloorballTeamAddedToSeasonEvent(Id, team.Id));
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
        AddDomainEvent(new FloorballTeamRemovedFromSeasonEvent(Id, team.Id));
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
        
        AddDomainEvent(new FloorballMatchAddedToSeasonEvent(Id, match.Id, match.HomeTeamId, match.AwayTeamId));
    }
} 
