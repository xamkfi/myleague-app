using Domain.DomainEvents.Common;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Domain.Entities.Hockey;
using Domain.Enums.Floorball;
using Domain.Enums.Hockey;
using Domain.EventSourcing;

public class Club : AggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime FoundingDate { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
    public Uri WebsiteUrl { get; private set; }
    public Uri LogoUrl { get; private set; }
    public string ContactEmail { get; private set; }

    private readonly List<FloorballTeam> _floorballTeams = new();
    private readonly List<HockeyTeam> _hockeyTeams = new();

    public IReadOnlyList<FloorballTeam> FloorballTeams => _floorballTeams;
    public IReadOnlyList<HockeyTeam> HockeyTeams => _hockeyTeams;

    private Club() { }

    public Club(
        string name,
        string city,
        string country,
        DateTime? foundingDate = null,
        Uri? websiteUrl = null,
        Uri? logoUrl = null,
        string? contactEmail = null)
    {
        ValidateRequired(name, nameof(name));
        ValidateRequired(city, nameof(city));
        ValidateRequired(country, nameof(country));

        Id = Guid.NewGuid();
        Name = name;
        City = city;
        Country = country;
        FoundingDate = foundingDate ?? DateTime.UtcNow;
        WebsiteUrl = websiteUrl ?? new Uri("https://example.com");
        LogoUrl = logoUrl ?? new Uri("https://example.com/logo.png");
        ContactEmail = contactEmail ?? "contact@example.com";

        AddDomainEvent(new ClubRegisteredEvent(Id, Name, City, Country, FoundingDate));
    }

    public void UpdateBasicInfo(string name, string city, string country)
    {
        ValidateRequired(name, nameof(name));
        ValidateRequired(city, nameof(city));
        ValidateRequired(country, nameof(country));

        Name = name;
        City = city;
        Country = country;

        AddDomainEvent(new ClubInfoUpdatedEvent(Id, Name, City, Country));
    }

    public void UpdateOnlinePresence(Uri? websiteUrl, Uri? logoUrl, string? contactEmail)
    {
        WebsiteUrl = websiteUrl ?? new Uri("https://example.com");
        LogoUrl = logoUrl ?? new Uri("https://example.com/logo.png");
        ContactEmail = contactEmail ?? "contact@example.com";
    }

    public FloorballTeam AddFloorballTeam(string name, FloorballDivision division, string homeArena, string primaryJerseyColor, string? secondaryColor = null)
    {
        ValidateRequired(name, nameof(name));
        ValidateRequired(homeArena, nameof(homeArena));
        ValidateRequired(primaryJerseyColor, nameof(primaryJerseyColor));

        var team = new FloorballTeam(name, division, this, homeArena, primaryJerseyColor, secondaryColor);
        _floorballTeams.Add(team);
        return team;
    }

    public bool RemoveFloorballTeam(Guid teamId)
    {
        FloorballTeam? team = _floorballTeams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return false;

        if (team.HasActiveMembers)
            throw new InvalidOperationException($"Cannot remove team {team.Name} as it has active members.");

        _floorballTeams.Remove(team);
        AddDomainEvent(new FloorballTeamRemovedEvent(Id, teamId));
        return true;
    }

    public IEnumerable<FloorballTeam> GetFloorballTeamsByDivision(FloorballDivision division) =>
        _floorballTeams.Where(t => t.Division == division);

    public HockeyTeam AddHockeyTeam(string name, HockeyDivision division, string homeArena, string primaryJerseyColor, string? secondaryColor = null)
    {
        ValidateRequired(name, nameof(name));
        ValidateRequired(homeArena, nameof(homeArena));
        ValidateRequired(primaryJerseyColor, nameof(primaryJerseyColor));

        HockeyTeam team = new HockeyTeam(name, division, this, homeArena, primaryJerseyColor, secondaryColor);
        _hockeyTeams.Add(team);
        return team;
    }

    public bool RemoveHockeyTeam(Guid teamId)
    {
        HockeyTeam? team = _hockeyTeams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return false;

        if (team.HasActiveMembers)
            throw new InvalidOperationException($"Cannot remove team {team.Name} as it has active members.");

        _hockeyTeams.Remove(team);
        return true;
    }

    public IEnumerable<HockeyTeam> GetHockeyTeamsByDivision(HockeyDivision division) =>
        _hockeyTeams.Where(t => t.Division == division);

    private static void ValidateRequired(string? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
    }
}
