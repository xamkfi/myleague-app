using Domain.ValueObjects.Floorball;
using Domain.Entities;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Enums.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball team within a club
/// </summary>
public class FloorballTeam : BaseEntity
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
    public Division? Division { get; private set; }

    /// <summary>
    /// Gets the ID of the division this team belongs to
    /// </summary>
    public Guid? DivisionId { get; private set; }

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
    /// Gets the logo URL of the team
    /// </summary>
    public Uri? LogoUrl { get; private set; }

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
        LogoUrl = null; // Default to null
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
    /// <param name="logoUrl">The team's logo URL (optional)</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballTeam(
        string name, 
        Guid? divisionId, 
        Club club,
        string? homeArena,
        string? primaryJerseyColor,
        TeamCategory teamCategory,
        string? secondaryJerseyColor = null,
        string? shortName = null,
        Uri? logoUrl = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(club);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be null or empty.", nameof(name));
        Id = Guid.NewGuid();

        Name = name;
        ShortName = string.Empty;
        ApplyShortName(shortName, name);

        DivisionId = divisionId;
        Division = default!;
        Club = club;
        ClubId = club.Id;
        // HomeArena and PrimaryJerseyColor are optional metadata. Tournament-only teams often
        // have no permanent home arena, so we store an empty string rather than reject the team.
        HomeArena = homeArena ?? string.Empty;
        PrimaryJerseyColor = primaryJerseyColor ?? string.Empty;
        SecondaryJerseyColor = secondaryJerseyColor ?? string.Empty;
        TeamCategory = teamCategory;
        LogoUrl = logoUrl;
        
    }

    /// <summary>
    /// Updates the team's short name (max 4 characters). If null/empty, defaults to first 3 chars of Name.
    /// </summary>
    /// <param name="shortName">New short name</param>
    public void UpdateShortName(string? shortName)
    {
        ApplyShortName(shortName, Name);
    }

    private void ApplyShortName(string? shortName, string baseName)
    {
        if (!string.IsNullOrWhiteSpace(shortName))
        {
            if (shortName.Length > 4)
                throw new ArgumentException("Short name cannot exceed 4 characters.", nameof(shortName));

            ShortName = shortName.ToUpperInvariant();
        }
        else
        {
            // Default to 3 characters when not provided
            ShortName = baseName.Length > 3 ? baseName[..3].ToUpperInvariant() : baseName.ToUpperInvariant();
        }
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
    public void UpdateDivision(Guid? divisionId)
    {
        DivisionId = divisionId;
    }
    
    /// <summary>
    /// Updates the team's home arena
    /// </summary>
    /// <param name="homeArena">The new home arena (nullable — empty stored as empty string)</param>
    public void UpdateHomeArena(string? homeArena)
    {
        HomeArena = homeArena ?? string.Empty;
    }

    /// <summary>
    /// Updates the team's jersey colors. Both colors are optional metadata; an empty/null value
    /// is stored as an empty string rather than rejected.
    /// </summary>
    /// <param name="primaryColor">The new primary jersey color (nullable)</param>
    /// <param name="secondaryColor">The new secondary jersey color (nullable)</param>
    public void UpdateJerseyColors(string? primaryColor, string? secondaryColor)
    {
        PrimaryJerseyColor = primaryColor ?? string.Empty;
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
    /// Updates the team's logo URL
    /// </summary>
    /// <param name="logoUrl">The new logo URL (optional)</param>
    public void UpdateLogo(Uri? logoUrl)
    {
        LogoUrl = logoUrl;
    }

    /// <summary>
    /// Gets the effective logo URL for the team, using the club's logo as fallback
    /// </summary>
    /// <param name="clubLogoUrl">The club's logo URL to use as fallback</param>
    /// <returns>The team's logo URL or the club's logo URL if team logo is not set</returns>
    public Uri? GetEffectiveLogoUrl(Uri? clubLogoUrl)
    {
        return LogoUrl ?? clubLogoUrl;
    }

    /// <summary>
    /// Adds a player to the team's roster
    /// </summary>
    /// <param name="player">The player to add</param>
    /// <param name="position">The player's position in the team</param>
    /// <param name="jerseyNumber">The player's actually-assigned jersey number</param>
    /// <param name="requestedJerseyNumber">
    /// The jersey number the caller originally wanted. When it differs from
    /// <paramref name="jerseyNumber"/>, the resulting roster entry is flagged via
    /// <see cref="FloorballTeamPlayer.HasJerseyNumberSubstituted"/> so the roster UI can
    /// highlight it for admin review. Pass <c>null</c> (or the same value as
    /// <paramref name="jerseyNumber"/>) when there was no substitution.
    /// </param>
    /// <exception cref="InvalidOperationException">Thrown when the player is already in the roster</exception>
    public void AddPlayer(
        FloorballPlayer player,
        FloorballPosition position,
        int? jerseyNumber = null,
        int? requestedJerseyNumber = null,
        Guid? competitionId = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (HasActiveRosterMembership(player.Id, competitionId))
            throw new InvalidOperationException($"Player with ID {player.Id} is already in the roster.");
        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, competitionId))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");
        FloorballTeamPlayer teamPlayer = new FloorballTeamPlayer(
            Id, player.Id, position, jerseyNumber, requestedJerseyNumber, competitionId);
        _roster.Add(teamPlayer);
    }

    /// <summary>
    /// Removes a player from one roster scope. When <paramref name="competitionId"/> is omitted
    /// and the player has several scoped rows, all of them are removed.
    /// </summary>
    public void RemovePlayer(Guid playerId, Guid? competitionId = null)
    {
        if (competitionId.HasValue)
        {
            FloorballTeamPlayer teamPlayer = GetRosterEntry(playerId, competitionId);
            _roster.Remove(teamPlayer);
            return;
        }

        List<FloorballTeamPlayer> rows = _roster.Where(p => p.PlayerId == playerId).ToList();
        if (rows.Count == 0)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");
        foreach (FloorballTeamPlayer row in rows)
            _roster.Remove(row);
    }

    public void UpdatePlayerPosition(Guid playerId, FloorballPosition newPosition, Guid? competitionId = null)
    {
        GetRosterEntry(playerId, competitionId).UpdatePosition(newPosition);
    }

    public void UpdatePlayerJerseyNumber(Guid playerId, int? jerseyNumber, Guid? competitionId = null)
    {
        FloorballTeamPlayer teamPlayer = GetRosterEntry(playerId, competitionId);
        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, teamPlayer.CompetitionId, teamPlayer.Id))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
    }

    public void UpdateTeamPlayer(
        Guid playerId,
        FloorballPosition position,
        int? jerseyNumber,
        bool isActive,
        Guid? competitionId = null)
    {
        FloorballTeamPlayer teamPlayer = GetRosterEntry(playerId, competitionId);
        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, teamPlayer.CompetitionId, teamPlayer.Id))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");

        teamPlayer.UpdatePosition(position);
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
        teamPlayer.SetActiveStatus(isActive);
    }

    public IReadOnlyCollection<FloorballTeamPlayer> GetActiveRoster(Guid? competitionId) =>
        _roster.Where(p => p.IsActive && p.CompetitionId == competitionId).ToList().AsReadOnly();

    public bool HasActiveRosterMembership(Guid playerId, Guid? competitionId) =>
        _roster.Any(p => p.IsActive && p.PlayerId == playerId && p.CompetitionId == competitionId);

    public bool IsPlayerOnRoster(Guid playerId, Guid? competitionId)
    {
        if (HasActiveRosterMembership(playerId, competitionId))
            return true;
        if (competitionId.HasValue && !_roster.Any(p => p.IsActive && p.CompetitionId == competitionId))
            return HasActiveRosterMembership(playerId, null);
        return false;
    }

    /// <summary>
    /// Latest competition that already has roster rows, excluding <paramref name="excludeCompetitionId"/>.
    /// Returns <c>null</c> when only a base roster (or nothing) exists.
    /// </summary>
    public Guid? FindLatestRosterCompetitionId(Guid? excludeCompetitionId = null)
    {
        FloorballTeamPlayer? latest = _roster
            .Where(p => p.IsActive && p.CompetitionId.HasValue && p.CompetitionId != excludeCompetitionId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .FirstOrDefault();
        return latest?.CompetitionId;
    }

    /// <summary>
    /// Copies active players from one roster scope into another. Conflicting jersey numbers
    /// are dropped so the copy can complete.
    /// </summary>
    public int CopyRosterToCompetition(Guid? sourceCompetitionId, Guid targetCompetitionId)
    {
        if (targetCompetitionId == Guid.Empty)
            throw new ArgumentException("Target competition id cannot be empty.", nameof(targetCompetitionId));
        if (sourceCompetitionId == targetCompetitionId)
            return 0;

        int copied = 0;
        foreach (FloorballTeamPlayer row in GetActiveRoster(sourceCompetitionId))
        {
            if (HasActiveRosterMembership(row.PlayerId, targetCompetitionId))
                continue;

            int? jersey = row.JerseyNumber;
            if (jersey.HasValue && IsJerseyNumberTaken(jersey.Value, targetCompetitionId))
                jersey = null;

            _roster.Add(new FloorballTeamPlayer(
                Id, row.PlayerId, row.Position, jersey, row.RequestedJerseyNumber, targetCompetitionId));
            copied++;
        }

        return copied;
    }

    private bool IsJerseyNumberTaken(int jerseyNumber, Guid? competitionId, Guid? excludeRosterRowId = null) =>
        _roster.Any(p =>
            p.IsActive &&
            p.CompetitionId == competitionId &&
            p.JerseyNumber == jerseyNumber &&
            p.Id != excludeRosterRowId);

    private FloorballTeamPlayer GetRosterEntry(Guid playerId, Guid? competitionId)
    {
        IEnumerable<FloorballTeamPlayer> matches = _roster.Where(p => p.PlayerId == playerId);
        FloorballTeamPlayer? teamPlayer = competitionId.HasValue
            ? matches.FirstOrDefault(p => p.CompetitionId == competitionId)
            : matches.Count() == 1
                ? matches.First()
                : matches.FirstOrDefault(p => p.CompetitionId == null);

        if (teamPlayer == null)
        {
            if (!competitionId.HasValue && matches.Count() > 1)
                throw new InvalidOperationException("Player is on multiple competition rosters; specify a competition.");
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");
        }

        return teamPlayer;
    }
} 
