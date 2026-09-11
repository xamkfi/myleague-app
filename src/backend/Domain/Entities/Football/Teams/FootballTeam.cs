using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Enums.Football;

namespace Domain.Entities.Football.Teams;

/// <summary>
/// A football team belonging to a club.
/// </summary>
public class FootballTeam : BaseEntity
{
    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public Division? Division { get; private set; }
    public Guid? DivisionId { get; private set; }
    public Club Club { get; private set; }
    public Guid ClubId { get; private set; }
    public TeamCategory TeamCategory { get; private set; }
    public IReadOnlyCollection<FootballTeamPlayer> Roster => _roster.AsReadOnly();
    private readonly List<FootballTeamPlayer> _roster = new();
    public bool HasActiveMembers => _roster.Count > 0 && _roster.Any(p => p.IsActive);
    public string HomeArena { get; private set; }
    public string PrimaryJerseyColor { get; private set; }
    public string SecondaryJerseyColor { get; private set; }
    public Uri? LogoUrl { get; private set; }

    private FootballTeam()
    {
        Name = string.Empty;
        ShortName = string.Empty;
        Club = null!;
        HomeArena = string.Empty;
        PrimaryJerseyColor = string.Empty;
        SecondaryJerseyColor = string.Empty;
        TeamCategory = TeamCategory.Adult;
    }

    public FootballTeam(
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

        Name = name;
        ShortName = string.Empty;
        ApplyShortName(shortName, name);
        DivisionId = divisionId;
        Division = default!;
        Club = club;
        ClubId = club.Id;
        HomeArena = homeArena ?? string.Empty;
        PrimaryJerseyColor = primaryJerseyColor ?? string.Empty;
        SecondaryJerseyColor = secondaryJerseyColor ?? string.Empty;
        TeamCategory = teamCategory;
        LogoUrl = logoUrl;
    }

    public void UpdateShortName(string? shortName) => ApplyShortName(shortName, Name);

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
            ShortName = baseName.Length > 3 ? baseName[..3].ToUpperInvariant() : baseName.ToUpperInvariant();
        }
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be null or empty.", nameof(name));
        Name = name;
    }

    public void UpdateDivision(Guid? divisionId) => DivisionId = divisionId;
    public void UpdateHomeArena(string? homeArena) => HomeArena = homeArena ?? string.Empty;

    public void UpdateJerseyColors(string? primaryColor, string? secondaryColor)
    {
        PrimaryJerseyColor = primaryColor ?? string.Empty;
        SecondaryJerseyColor = secondaryColor ?? string.Empty;
    }

    public void UpdateTeamCategory(TeamCategory teamCategory) => TeamCategory = teamCategory;
    public void UpdateLogo(Uri? logoUrl) => LogoUrl = logoUrl;
    public Uri? GetEffectiveLogoUrl(Uri? clubLogoUrl) => LogoUrl ?? clubLogoUrl;

    public void AddPlayer(
        FootballPlayer player,
        FootballPosition position,
        int? jerseyNumber = null,
        int? requestedJerseyNumber = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (_roster.Count > 0 && _roster.Any(p => p.PlayerId == player.Id))
            throw new InvalidOperationException($"Player with ID {player.Id} is already in the roster.");
        if (jerseyNumber.HasValue && _roster.Count > 0 && _roster.Any(p => p.JerseyNumber == jerseyNumber))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");
        _roster.Add(new FootballTeamPlayer(Id, player.Id, position, jerseyNumber, requestedJerseyNumber));
    }

    public void RemovePlayer(Guid playerId)
    {
        FootballTeamPlayer? teamPlayer = _roster.FirstOrDefault(p => p.PlayerId == playerId);
        if (teamPlayer == null)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");
        _roster.Remove(teamPlayer);
    }

    public void UpdatePlayerPosition(Guid playerId, FootballPosition newPosition)
    {
        FootballTeamPlayer teamPlayer = GetRosterEntry(playerId);
        teamPlayer.UpdatePosition(newPosition);
    }

    public void UpdatePlayerJerseyNumber(Guid playerId, int? jerseyNumber)
    {
        FootballTeamPlayer teamPlayer = GetRosterEntry(playerId);
        if (jerseyNumber.HasValue && _roster.Any(p => p.JerseyNumber == jerseyNumber && p.PlayerId != playerId))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
    }

    public void UpdateTeamPlayer(Guid playerId, FootballPosition position, int? jerseyNumber, bool isActive)
    {
        FootballTeamPlayer teamPlayer = GetRosterEntry(playerId);
        if (jerseyNumber.HasValue && _roster.Any(p => p.JerseyNumber == jerseyNumber && p.PlayerId != playerId))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned to another player.");
        teamPlayer.UpdatePosition(position);
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
        teamPlayer.SetActiveStatus(isActive);
    }

    private FootballTeamPlayer GetRosterEntry(Guid playerId)
    {
        FootballTeamPlayer? teamPlayer = _roster.FirstOrDefault(p => p.PlayerId == playerId);
        if (teamPlayer == null)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");
        return teamPlayer;
    }
}
