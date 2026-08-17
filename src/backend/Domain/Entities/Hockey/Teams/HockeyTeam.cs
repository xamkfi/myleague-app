using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Enums.Hockey.Teams;
using Domain.ValueObjects.Hockey.Rules;

namespace Domain.Entities.Hockey.Teams;

/// <summary>
/// Hockey team aggregate owned by a <see cref="Club"/>. Manages roster, default lines and staff.
/// </summary>
public class HockeyTeam : BaseEntity
{
    public Guid ClubId { get; private set; }
    public Club Club { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string ShortName { get; private set; } = string.Empty;
    public Guid? DivisionId { get; private set; }
    public TeamCategory TeamCategory { get; private set; }
    public string HomeArena { get; private set; } = string.Empty;
    public string PrimaryJerseyColor { get; private set; } = string.Empty;
    public string SecondaryJerseyColor { get; private set; } = string.Empty;
    public Uri? LogoUrl { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<HockeyTeamPlayer> Roster => _roster.AsReadOnly();
    private readonly List<HockeyTeamPlayer> _roster = new();

    public IReadOnlyCollection<HockeyLine> Lines => _lines.AsReadOnly();
    private readonly List<HockeyLine> _lines = new();

    public IReadOnlyCollection<HockeyTeamStaff> StaffMembers => _staff.AsReadOnly();
    private readonly List<HockeyTeamStaff> _staff = new();

    public bool HasActiveMembers => _roster.Any(p => p.IsActive) || _staff.Any(s => s.IsActive);

    private HockeyTeam() { }

    public HockeyTeam(
        string name,
        Club club,
        TeamCategory teamCategory,
        Guid? divisionId = null,
        string? homeArena = null,
        string? primaryJerseyColor = null,
        string? secondaryJerseyColor = null,
        string? shortName = null,
        Uri? logoUrl = null)
    {
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be null or empty.", nameof(name));

        Club = club;
        ClubId = club.Id;
        Name = name;
        ApplyShortName(shortName, name);
        DivisionId = divisionId;
        TeamCategory = teamCategory;
        HomeArena = homeArena ?? string.Empty;
        PrimaryJerseyColor = primaryJerseyColor ?? string.Empty;
        SecondaryJerseyColor = secondaryJerseyColor ?? string.Empty;
        LogoUrl = logoUrl;
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be null or empty.", nameof(name));
        Name = name;
    }

    public void UpdateShortName(string? shortName) => ApplyShortName(shortName, Name);

    public void UpdateDivision(Guid? divisionId) => DivisionId = divisionId;

    public void UpdateTeamCategory(TeamCategory teamCategory) => TeamCategory = teamCategory;

    public void UpdateHomeArena(string? homeArena) => HomeArena = homeArena ?? string.Empty;

    public void UpdateJerseyColors(string? primaryColor, string? secondaryColor)
    {
        PrimaryJerseyColor = primaryColor ?? string.Empty;
        SecondaryJerseyColor = secondaryColor ?? string.Empty;
    }

    public void UpdateLogo(Uri? logoUrl) => LogoUrl = logoUrl;

    public void SetActiveStatus(bool isActive) => IsActive = isActive;

    public Uri? GetEffectiveLogoUrl(Uri? clubLogoUrl) => LogoUrl ?? clubLogoUrl;

    public HockeyTeamPlayer AddPlayer(
        HockeyPlayer player,
        HockeyPosition position,
        Guid? competitionId = null,
        int? jerseyNumber = null,
        int? requestedJerseyNumber = null,
        HockeyRosterStatus rosterStatus = HockeyRosterStatus.Active,
        HockeyRosterRules? rosterRules = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        ValidateRosterStatus(rosterStatus, rosterRules);

        if (HasActiveRosterMembership(player.Id, competitionId))
            throw new InvalidOperationException("Player is already on this roster.");

        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, competitionId))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned.");

        HockeyTeamPlayer teamPlayer = new(
            Id, player.Id, position, competitionId, jerseyNumber, requestedJerseyNumber,
            rosterStatus: rosterStatus);
        _roster.Add(teamPlayer);
        return teamPlayer;
    }

    public void RemovePlayer(Guid playerId, Guid? competitionId = null)
    {
        HockeyTeamPlayer teamPlayer = GetActiveTeamPlayer(playerId, competitionId)
            ?? throw new InvalidOperationException("Player is not on this roster.");

        if (_lines.Any(l => l.IsActive && l.Players.Any(p => p.TeamPlayerId == teamPlayer.Id)))
            throw new InvalidOperationException("Cannot remove a player who is assigned to an active line.");

        teamPlayer.Leave();
    }

    public void UpdateTeamPlayer(
        Guid playerId,
        HockeyPosition position,
        int? jerseyNumber,
        HockeyRosterStatus rosterStatus,
        HockeyCaptainRole captainRole,
        Guid? competitionId = null,
        HockeyRosterRules? rosterRules = null)
    {
        HockeyTeamPlayer teamPlayer = GetActiveTeamPlayer(playerId, competitionId)
            ?? throw new InvalidOperationException("Player is not on this roster.");

        ValidateRosterStatus(rosterStatus, rosterRules);
        ValidateCaptainRole(teamPlayer, captainRole, competitionId, rosterRules);

        if (jerseyNumber.HasValue && IsJerseyNumberTaken(jerseyNumber.Value, competitionId, teamPlayer.Id))
            throw new InvalidOperationException($"Jersey number {jerseyNumber} is already assigned.");

        teamPlayer.UpdatePosition(position);
        teamPlayer.UpdateJerseyNumber(jerseyNumber);
        teamPlayer.UpdateRosterStatus(rosterStatus);
        teamPlayer.UpdateCaptainRole(captainRole);
    }

    public HockeyLine AddLine(
        string name,
        int lineNumber,
        HockeyLineType lineType,
        Guid? competitionId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Line name cannot be null or empty.", nameof(name));

        HockeyLine line = new(Id, name, lineNumber, lineType, competitionId);
        _lines.Add(line);
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        HockeyLine? line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException("Line is not part of this team.");

        line.Deactivate();
    }

    public HockeyLinePlayer AddPlayerToLine(
        Guid lineId,
        Guid teamPlayerId,
        HockeyLineSlot slot,
        int order)
    {
        HockeyLine line = _lines.FirstOrDefault(l => l.Id == lineId && l.IsActive)
            ?? throw new InvalidOperationException("Line is not part of this team.");

        HockeyTeamPlayer teamPlayer = _roster.FirstOrDefault(p => p.Id == teamPlayerId && p.IsActive)
            ?? throw new InvalidOperationException("Team player is not on this roster.");

        if (line.CompetitionId != teamPlayer.CompetitionId)
            throw new InvalidOperationException("Line and team player must belong to the same roster scope.");

        return line.AddPlayer(teamPlayerId, slot, order);
    }

    public void RemovePlayerFromLine(Guid lineId, Guid teamPlayerId)
    {
        HockeyLine line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException("Line is not part of this team.");

        line.RemovePlayer(teamPlayerId);
    }

    public HockeyTeamStaff AddStaff(Guid personId, HockeyTeamStaffRole role, Guid? competitionId = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person id cannot be empty.", nameof(personId));

        if (_staff.Any(s => s.IsActive && s.PersonId == personId && s.Role == role && s.CompetitionId == competitionId))
            throw new InvalidOperationException("Staff member with this role is already active on this team.");

        HockeyTeamStaff staff = new(personId, Id, role, competitionId);
        _staff.Add(staff);
        return staff;
    }

    public void RemoveStaff(Guid staffId)
    {
        HockeyTeamStaff staff = _staff.FirstOrDefault(s => s.Id == staffId && s.IsActive)
            ?? throw new InvalidOperationException("Staff member is not part of this team.");

        staff.Leave();
    }

    private HockeyTeamPlayer? GetActiveTeamPlayer(Guid playerId, Guid? competitionId) =>
        _roster.FirstOrDefault(p => p.PlayerId == playerId && p.CompetitionId == competitionId && p.IsActive);

    private bool HasActiveRosterMembership(Guid playerId, Guid? competitionId) =>
        _roster.Any(p => p.PlayerId == playerId && p.CompetitionId == competitionId && p.IsActive);

    private bool IsJerseyNumberTaken(int jerseyNumber, Guid? competitionId, Guid? excludeTeamPlayerId = null) =>
        _roster.Any(p =>
            p.IsActive &&
            p.CompetitionId == competitionId &&
            p.JerseyNumber == jerseyNumber &&
            p.Id != excludeTeamPlayerId);

    private void ValidateRosterStatus(HockeyRosterStatus rosterStatus, HockeyRosterRules? rosterRules)
    {
        if (rosterRules is null)
            return;

        if (rosterStatus is HockeyRosterStatus.Guest or HockeyRosterStatus.Loaned && !rosterRules.AllowGuestPlayers)
            throw new InvalidOperationException("Guest and loaned players are not allowed by roster rules.");
    }

    private void ValidateCaptainRole(
        HockeyTeamPlayer teamPlayer,
        HockeyCaptainRole captainRole,
        Guid? competitionId,
        HockeyRosterRules? rosterRules)
    {
        if (captainRole == HockeyCaptainRole.None)
            return;

        if (rosterRules is not null &&
            captainRole == HockeyCaptainRole.Captain &&
            teamPlayer.Position == HockeyPosition.Goalie &&
            !rosterRules.CanGoalieBeCaptain)
        {
            throw new InvalidOperationException("Goalies cannot be captain under current roster rules.");
        }

        IEnumerable<HockeyTeamPlayer> scope = _roster.Where(p =>
            p.IsActive && p.CompetitionId == competitionId && p.Id != teamPlayer.Id);

        if (rosterRules is not null)
        {
            int captainCount = scope.Count(p => p.CaptainRole == HockeyCaptainRole.Captain);
            int alternateCount = scope.Count(p => p.CaptainRole == HockeyCaptainRole.AlternateCaptain);

            if (captainRole == HockeyCaptainRole.Captain && captainCount >= rosterRules.MaxCaptains)
                throw new InvalidOperationException("Maximum number of captains has been reached.");

            if (captainRole == HockeyCaptainRole.AlternateCaptain && alternateCount >= rosterRules.MaxAlternateCaptains)
                throw new InvalidOperationException("Maximum number of alternate captains has been reached.");
        }
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
            ShortName = baseName.Length > 3 ? baseName[..3].ToUpperInvariant() : baseName.ToUpperInvariant();
        }
    }
}
