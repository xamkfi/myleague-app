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
        int? requestedJerseyNumber = null,
        Guid? competitionId = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (HasActiveRosterMembership(player.Id, competitionId))
            throw new InvalidOperationException($"Player with ID {player.Id} is already in the roster.");
        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, competitionId))
            throw new InvalidOperationException(JerseyTakenMessage(jerseyNumber.Value));
        _roster.Add(new FootballTeamPlayer(Id, player.Id, position, jerseyNumber, requestedJerseyNumber, competitionId));
    }

    public void RemovePlayer(Guid playerId, Guid? competitionId = null)
    {
        if (competitionId.HasValue)
        {
            _roster.Remove(GetRosterEntry(playerId, competitionId));
            return;
        }

        List<FootballTeamPlayer> rows = _roster.Where(p => p.PlayerId == playerId).ToList();
        if (rows.Count == 0)
            throw new InvalidOperationException($"Player with ID {playerId} is not in the roster.");
        foreach (FootballTeamPlayer row in rows)
            _roster.Remove(row);
    }

    public void UpdatePlayerPosition(Guid playerId, FootballPosition newPosition, Guid? competitionId = null) =>
        GetRosterEntry(playerId, competitionId).UpdatePosition(newPosition);

    public void UpdatePlayerJerseyNumber(Guid playerId, int? jerseyNumber, Guid? competitionId = null)
    {
        FootballTeamPlayer teamPlayer = GetRosterEntry(playerId, competitionId);
        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, teamPlayer.CompetitionId, teamPlayer.Id))
            throw new InvalidOperationException(JerseyTakenMessage(jerseyNumber.Value));
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
    }

    public void UpdateTeamPlayer(
        Guid playerId,
        FootballPosition position,
        int? jerseyNumber,
        bool isActive,
        Guid? competitionId = null)
    {
        FootballTeamPlayer teamPlayer = GetRosterEntry(playerId, competitionId);
        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, teamPlayer.CompetitionId, teamPlayer.Id))
            throw new InvalidOperationException(JerseyTakenMessage(jerseyNumber.Value));
        teamPlayer.UpdatePosition(position);
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
        teamPlayer.SetActiveStatus(isActive);
    }

    public IReadOnlyCollection<FootballTeamPlayer> GetActiveRoster(Guid? competitionId) =>
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

    public Guid? FindLatestRosterCompetitionId(Guid? excludeCompetitionId = null)
    {
        FootballTeamPlayer? latest = _roster
            .Where(p => p.IsActive && p.CompetitionId.HasValue && p.CompetitionId != excludeCompetitionId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .FirstOrDefault();
        return latest?.CompetitionId;
    }

    public int CopyRosterToCompetition(Guid? sourceCompetitionId, Guid targetCompetitionId)
    {
        if (targetCompetitionId == Guid.Empty)
            throw new ArgumentException("Target competition id cannot be empty.", nameof(targetCompetitionId));
        if (sourceCompetitionId == targetCompetitionId)
            return 0;

        int copied = 0;
        foreach (FootballTeamPlayer row in GetActiveRoster(sourceCompetitionId))
        {
            if (HasActiveRosterMembership(row.PlayerId, targetCompetitionId))
                continue;

            int? jersey = row.JerseyNumber;
            if (jersey.HasValue && IsJerseyNumberTaken(jersey.Value, targetCompetitionId))
                jersey = null;

            _roster.Add(new FootballTeamPlayer(
                Id, row.PlayerId, row.Position, jersey, row.RequestedJerseyNumber, targetCompetitionId));
            copied++;
        }

        return copied;
    }

    private static string JerseyTakenMessage(int jerseyNumber) =>
        $"Jersey number {jerseyNumber} is already used by another player on this team in this competition.";

    /// <summary>
    /// Jersey numbers are unique per team and competition for every roster row.
    /// An inactive membership still holds its number.
    /// </summary>
    private bool IsJerseyNumberTaken(int jerseyNumber, Guid? competitionId, Guid? excludeRosterRowId = null) =>
        _roster.Any(p =>
            p.CompetitionId == competitionId &&
            p.JerseyNumber == jerseyNumber &&
            p.Id != excludeRosterRowId);

    private FootballTeamPlayer GetRosterEntry(Guid playerId, Guid? competitionId)
    {
        IEnumerable<FootballTeamPlayer> matches = _roster.Where(p => p.PlayerId == playerId);
        FootballTeamPlayer? teamPlayer = competitionId.HasValue
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
