using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.EventSourcing;

namespace Domain.Entities.Common;

/// <summary>
/// Represents a sports club that can have multiple floorball teams
/// </summary>
public class Club : AggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of the club
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the club
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets the founding date of the club
    /// </summary>
    public DateTime FoundingDate { get; private set; }

    /// <summary>
    /// Gets the city where the club is based
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets the country where the club is based
    /// </summary>
    public required string Country { get; set; }
    
    /// <summary>
    /// Gets the club's official website URL
    /// </summary>
    public required Uri WebsiteUrl { get; set; }
    
    /// <summary>
    /// Gets the club's logo URL
    /// </summary>
    public required Uri LogoUrl { get; set; }
    
    /// <summary>
    /// Gets the primary contact email for the club
    /// </summary>
    public required string ContactEmail { get; set; }
    
    /// <summary>
    /// Gets the floorball teams associated with this club
    /// </summary>
    public IReadOnlyCollection<FloorballTeam> FloorballTeams => _floorballTeams.AsReadOnly();
    private readonly List<FloorballTeam> _floorballTeams = new();

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private Club()
    {
        Id = Guid.NewGuid();
        _floorballTeams = new List<FloorballTeam>();
    }

    /// <summary>
    /// Initializes a new instance of the Club class
    /// </summary>
    /// <param name="name">The name of the club</param>
    /// <param name="city">The city where the club is based</param>
    /// <param name="country">The country where the club is based</param>
    /// <param name="foundingDate">The founding date of the club</param>
    /// <param name="websiteUrl">The club's official website URL</param>
    /// <param name="logoUrl">The club's logo URL</param>
    /// <param name="contactEmail">The primary contact email for the club</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public Club(
        string name,
        string city,
        string country,
        DateTime? foundingDate = null,
        Uri? websiteUrl = null,
        Uri? logoUrl = null,
        string? contactEmail = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(country);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Club name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        if (foundingDate.HasValue && foundingDate.Value > DateTime.UtcNow)
            throw new ArgumentException("Founding date cannot be in the future.", nameof(foundingDate));

        Id = Guid.NewGuid();
        Name = name;
        City = city;
        Country = country;
        FoundingDate = foundingDate ?? DateTime.UtcNow;
        WebsiteUrl = websiteUrl ?? new Uri("https://example.com");
        LogoUrl = logoUrl ?? new Uri("https://example.com/logo.png");
        ContactEmail = contactEmail ?? "contact@example.com";
        _floorballTeams = new List<FloorballTeam>();
    }

    /// <summary>
    /// Updates the club's name
    /// </summary>
    /// <param name="name">The new name</param>
    /// <exception cref="ArgumentException">Thrown when the name is invalid</exception>
    public void UpdateName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Club name cannot be empty.", nameof(name));

        Name = name;
    }

    /// <summary>
    /// Updates the club's basic information
    /// </summary>
    /// <param name="name">The new name of the club</param>
    /// <param name="city">The new city where the club is based</param>
    /// <param name="country">The new country where the club is based</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public void UpdateBasicInfo(string name, string city, string country)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(country);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Club name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        Name = name;
        City = city;
        Country = country;
    }
    
    /// <summary>
    /// Updates the club's online presence information
    /// </summary>
    /// <param name="websiteUrl">The new website URL</param>
    /// <param name="logoUrl">The new logo URL</param>
    /// <param name="contactEmail">The new contact email</param>
    public void UpdateOnlinePresence(Uri? websiteUrl, Uri? logoUrl, string? contactEmail)
    {
        WebsiteUrl = websiteUrl ?? new Uri("https://example.com");
        LogoUrl = logoUrl ?? new Uri("https://example.com/logo.png");
        ContactEmail = contactEmail ?? "contact@example.com";
    }

    /// <summary>
    /// Adds a new floorball team to the club
    /// </summary>
    /// <param name="name">The name of the team</param>
    /// <param name="division">The division level of the team</param>
    /// <param name="homeArena">The team's home arena</param>
    /// <param name="primaryJerseyColor">The team's primary jersey color</param>
    /// <param name="secondaryJerseyColor">The team's secondary jersey color</param>
    /// <returns>The newly created floorball team</returns>
    public FloorballTeam AddFloorballTeam(
        string name, 
        FloorballDivision division, 
        string homeArena,
        string primaryJerseyColor,
        string? secondaryJerseyColor = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(homeArena);
        ArgumentNullException.ThrowIfNull(primaryJerseyColor);

        var team = new FloorballTeam(name, division, this, homeArena, primaryJerseyColor, secondaryJerseyColor);
        _floorballTeams.Add(team);
        return team;
    }

    /// <summary>
    /// Removes a floorball team from the club
    /// </summary>
    /// <param name="teamId">The ID of the team to remove</param>
    /// <returns>True if the team was removed, false if the team was not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when the team has active members</exception>
    public bool RemoveFloorballTeam(Guid teamId)
    {
        FloorballTeam? team = _floorballTeams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
            return false;
        
        if (team.HasActiveMembers)
            throw new InvalidOperationException($"Cannot remove team {team.Name} as it has active members.");
        
        return _floorballTeams.Remove(team);
    }

    /// <summary>
    /// Gets floorball teams by division
    /// </summary>
    /// <param name="division">The division to filter by</param>
    /// <returns>Teams in the specified division</returns>
    public IEnumerable<FloorballTeam> GetFloorballTeamsByDivision(FloorballDivision division)
    {
        return _floorballTeams.Where(t => t.Division == division);
    }
} 
