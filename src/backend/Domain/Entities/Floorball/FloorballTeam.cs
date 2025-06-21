using Domain.ValueObjects.Floorball;
using Domain.Entities;
using Domain.EventSourcing;
using Domain.Entities.Common;
using Domain.DomainEvents.Floorball;
using Domain.Enums.Common;
using Domain.Enums.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball team within a club
/// </summary>
public class FloorballTeam : AggregateRoot
{
    /// <summary>
    /// Gets the name of the team
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the short name of the team, used for display purposes
    /// </summary>
    public string ShortName { get; private set; }

    /// <summary>
    /// Gets the division level of the team
    /// </summary>
    public Division Division { get; private set; }

    /// <summary>
    /// Gets the ID of the division this team belongs to
    /// </summary>
    public Guid DivisionId { get; private set; }

    /// <summary>
    /// Gets the club this team belongs to
    /// </summary>
    public Club Club { get; private set; }

    /// <summary>
    /// Gets the ID of the club this team belongs to
    /// </summary>
    public Guid ClubId { get; private set; }

    public TeamCategory TeamCategory { get; private set; }

    /// <summary>
    /// Gets the team's roster of players
    /// </summary>
    public IReadOnlyCollection<FloorballTeamPlayer> Roster => _roster.AsReadOnly();
    private readonly List<FloorballTeamPlayer> _roster = new();

    /// <summary>
    /// Gets whether the team has any active members
    /// </summary>
    public bool HasActiveMembers => _roster.Count > 0 && _roster.Any(p => p.IsActive);
    
    /// <summary>
    /// Gets the team's home arena
    /// </summary>
    public string HomeArena { get; private set; }
    
    /// <summary>
    /// Gets the team's primary jersey color
    /// </summary>
    public string PrimaryJerseyColor { get; private set; }
    
    /// <summary>
    /// Gets the team's secondary jersey color
    /// </summary>
    public string SecondaryJerseyColor { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTeam()
    {
        Id = Guid.NewGuid();
        _roster = new List<FloorballTeamPlayer>();
        Name = string.Empty;
        Club = null!; // Marked as non-nullable, but initialized to null for EF Core
        ClubId = Guid.Empty; // Default to empty Guid for EF Core
        HomeArena = string.Empty; // Default to an empty string
        PrimaryJerseyColor = string.Empty; // Default to an empty string
        SecondaryJerseyColor = string.Empty; // Default to an empty string
        Division = default!; // Default to None division
        DivisionId = Guid.Empty; // Default to empty Guid for EF Core
        ShortName = string.Empty; // Default to an empty string
        TeamCategory = TeamCategory.Adult; // Default to Adult category
    }

    /// <summary>
    /// Initializes a new instance of the FloorballTeam class
    /// </summary>
    /// <param name="name">The name of the team</param>
    /// <param name="division">The division level of the team</param>
    /// <param name="club">The club this team belongs to</param>
    /// <param name="homeArena">The team's home arena</param>
    /// <param name="primaryJerseyColor">The team's primary jersey color</param>
    /// <param name="teamCategory">The category of the team (Adult, Youth, Women)</param>
    /// <param name="secondaryJerseyColor">The team's secondary jersey color (optional)</param>
    /// <param name="shortName">The team's short name (optional)</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballTeam(
        string name, 
        Guid divisionId, 
        Club club,
        string homeArena,
        string primaryJerseyColor,
        TeamCategory teamCategory,
        string? secondaryJerseyColor = null,
        string? shortName = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(homeArena);
        ArgumentNullException.ThrowIfNull(primaryJerseyColor);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(homeArena))
            throw new ArgumentException("Home arena cannot be null or empty.", nameof(homeArena));
        if (string.IsNullOrWhiteSpace(primaryJerseyColor))
            throw new ArgumentException("Primary jersey color cannot be null or empty.", nameof(primaryJerseyColor));
        Id = Guid.NewGuid();

        Name = name;
        if(!string.IsNullOrWhiteSpace(shortName))
        {
            if(shortName.Length > 3)
                throw new ArgumentException("Short name cannot exceed 3 characters.", nameof(shortName));

            ShortName = shortName;
        }
        else
        {
            // Default to the first 3 characters of the name if no short name is provided
            ShortName = name.Length > 3 ? name.Substring(0, 3).ToUpperInvariant() : name.ToUpperInvariant();
        }

        DivisionId = divisionId;
        Division = default!;
        Club = club;
        ClubId = club.Id;
        HomeArena = homeArena;
        PrimaryJerseyColor = primaryJerseyColor;
        SecondaryJerseyColor = secondaryJerseyColor ?? string.Empty;
        TeamCategory = teamCategory;
        
        AddDomainEvent(new FloorballTeamRegisteredEvent(
            Id, 
            name, 
            divisionId, 
            club.Id, 
            homeArena, 
            primaryJerseyColor, 
            secondaryJerseyColor));
    }

    /// <summary>
    /// Updates the team's name
    /// </summary>
    /// <param name="name">The new name</param>
    /// <exception cref="ArgumentException">Thrown when the name is invalid</exception>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be null or empty.", nameof(name));

        Name = name;
    }
    
    /// <summary>
    /// Updates the team's division
    /// </summary>
    /// <param name="division">The new division</param>
    public void UpdateDivision(Guid divisionId)
    {
        DivisionId = divisionId;
    }
    
    /// <summary>
    /// Updates the team's home arena
    /// </summary>
    /// <param name="homeArena">The new home arena</param>
    /// <exception cref="ArgumentException">Thrown when the home arena is invalid</exception>
    public void UpdateHomeArena(string homeArena)
    {
        if (string.IsNullOrWhiteSpace(homeArena))
            throw new ArgumentException("Home arena cannot be null or empty.", nameof(homeArena));

        HomeArena = homeArena;
    }
    
    /// <summary>
    /// Updates the team's jersey colors
    /// </summary>
    /// <param name="primaryColor">The new primary jersey color</param>
    /// <param name="secondaryColor">The new secondary jersey color</param>
    /// <exception cref="ArgumentException">Thrown when the primary color is invalid</exception>
    public void UpdateJerseyColors(string primaryColor, string secondaryColor)
    {
        if (string.IsNullOrWhiteSpace(primaryColor))
            throw new ArgumentException("Primary jersey color cannot be null or empty.", nameof(primaryColor));

        PrimaryJerseyColor = primaryColor;
        SecondaryJerseyColor = secondaryColor ?? string.Empty;
    }

    /// <summary>
    /// Updates the team's category
    /// </summary>
    /// <param name="teamCategory">The new team category</param>
    public void UpdateTeamCategory(TeamCategory teamCategory)
    {
        TeamCategory = teamCategory;
    }

    /// <summary>
    /// Adds a player to the team's roster
    /// </summary>
    /// <param name="player">The player to add</param>
    /// <param name="position">The player's position in the team</param>
    /// <param name="jerseyNumber">The player's jersey number</param>
    /// <exception cref="InvalidOperationException">Thrown when the player is already in the roster</exception>
    public void AddPlayer(FloorballPlayer player, FloorballPosition position, int? jerseyNumber = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (_roster.Count > 0 && _roster.Any(p => p.PlayerId == player.Id))
            throw new InvalidOperationException($"Player with ID {player.Id} is already in the roster.");
        if (jerseyNumber.HasValue && _roster.Count > 0 && _roster.Any(p => p.JerseyNumber == jerseyNumber))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");
        var teamPlayer = new FloorballTeamPlayer(Id, player.Id, position, jerseyNumber);
        _roster.Add(teamPlayer);
        
        // Create and add a domain event for player addition
        AddDomainEvent(new FloorballPlayerAddedToTeamEvent(
            Id,
            player.Id,
            position,
            jerseyNumber));
    }

    /// <summary>
    /// Removes a player from the team's roster
    /// </summary>
    /// <param name="playerId">The ID of the player to remove</param>
    /// <exception cref="InvalidOperationException">Thrown when the player is not found</exception>
    public void RemovePlayer(Guid playerId)
    {
        FloorballTeamPlayer? teamPlayer = _roster.FirstOrDefault(p => p.PlayerId == playerId);
        if (teamPlayer == null)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");

        _roster.Remove(teamPlayer);
        
        // Create and add a domain event for player removal
        AddDomainEvent(new FloorballPlayerRemovedFromTeamEvent(
            Id,
            playerId));
    }

    /// <summary>
    /// Updates a player's position in the team
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="newPosition">The new position</param>
    /// <exception cref="InvalidOperationException">Thrown when the player is not found</exception>
    public void UpdatePlayerPosition(Guid playerId, FloorballPosition newPosition)
    {
        FloorballTeamPlayer? teamPlayer = _roster.FirstOrDefault(p => p.PlayerId == playerId);
        if (teamPlayer == null)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");

        teamPlayer.UpdatePosition(newPosition);
    }
    
    /// <summary>
    /// Updates a player's jersey number
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="jerseyNumber">The new jersey number</param>
    /// <exception cref="InvalidOperationException">Thrown when the player is not found or the jersey number is already taken</exception>
    public void UpdatePlayerJerseyNumber(Guid playerId, int? jerseyNumber)
    {
        FloorballTeamPlayer? teamPlayer = _roster.FirstOrDefault(p => p.PlayerId == playerId);
        if (teamPlayer == null)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");

        if (jerseyNumber.HasValue && _roster.Any(p => p.JerseyNumber == jerseyNumber && p.PlayerId != playerId))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");

        teamPlayer.UpdateJerseyNumber(jerseyNumber);
    }

    /// <summary>
    /// Updates a player's information in the team (position, jersey number, and active status)
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="position">The new position</param>
    /// <param name="jerseyNumber">The new jersey number</param>
    /// <param name="isActive">The new active status</param>
    /// <exception cref="InvalidOperationException">Thrown when the player is not found or the jersey number is already taken</exception>
    public void UpdateTeamPlayer(Guid playerId, FloorballPosition position, int? jerseyNumber, bool isActive)
    {
        FloorballTeamPlayer? teamPlayer = _roster.FirstOrDefault(p => p.PlayerId == playerId);
        if (teamPlayer == null)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");

        // Check if jersey number is already taken by another player
        if (jerseyNumber.HasValue && _roster.Any(p => p.JerseyNumber == jerseyNumber && p.PlayerId != playerId))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");

        // Update all properties
        teamPlayer.UpdatePosition(position);
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
        teamPlayer.SetActiveStatus(isActive);
        
        // Create and add a domain event for player update
        AddDomainEvent(new FloorballPlayerUpdatedInTeamEvent(
            Id,
            playerId,
            position,
            jerseyNumber,
            isActive));
    }
} 
