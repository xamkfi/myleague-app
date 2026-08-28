using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Teams;

/// <summary>
/// A player's membership on a <see cref="HockeyTeam"/>. <see cref="CompetitionId"/> is null
/// for the base roster or set for a competition-specific roster (season or tournament).
/// </summary>
public class HockeyTeamPlayer : BaseEntity
{
    public Guid TeamId { get; private set; }
    public HockeyTeam Team { get; private set; } = null!;
    public Guid PlayerId { get; private set; }
    public HockeyPlayer Player { get; private set; } = null!;
    public Guid? CompetitionId { get; private set; }
    public HockeyPosition Position { get; private set; }
    public HockeyCaptainRole CaptainRole { get; private set; }
    public HockeyRosterStatus RosterStatus { get; private set; }
    public int? JerseyNumber { get; private set; }
    public int? RequestedJerseyNumber { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public bool IsActive => LeftAt is null;

    public bool HasJerseyNumberSubstituted =>
        RequestedJerseyNumber.HasValue && RequestedJerseyNumber != JerseyNumber;

    public int GamesPlayed { get; private set; }
    public int Goals { get; private set; }
    public int Assists { get; private set; }
    public int Points => Goals + Assists;
    public int PenaltyMinutes { get; private set; }

    private HockeyTeamPlayer() { }

    internal HockeyTeamPlayer(
        Guid teamId,
        Guid playerId,
        HockeyPosition position,
        Guid? competitionId = null,
        int? jerseyNumber = null,
        int? requestedJerseyNumber = null,
        HockeyCaptainRole captainRole = HockeyCaptainRole.None,
        HockeyRosterStatus rosterStatus = HockeyRosterStatus.Active)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player id cannot be empty.", nameof(playerId));

        TeamId = teamId;
        PlayerId = playerId;
        Position = position;
        CompetitionId = competitionId;
        JerseyNumber = jerseyNumber;
        RequestedJerseyNumber = requestedJerseyNumber.HasValue && requestedJerseyNumber != jerseyNumber
            ? requestedJerseyNumber
            : null;
        CaptainRole = captainRole;
        RosterStatus = rosterStatus;
        JoinedAt = DateTime.UtcNow;
    }

    internal void Leave()
    {
        if (LeftAt is not null)
            return;

        LeftAt = DateTime.UtcNow;
        CaptainRole = HockeyCaptainRole.None;
    }

    internal void UpdatePosition(HockeyPosition position) => Position = position;

    internal void UpdateCaptainRole(HockeyCaptainRole captainRole) => CaptainRole = captainRole;

    internal void UpdateRosterStatus(HockeyRosterStatus rosterStatus) => RosterStatus = rosterStatus;

    internal void UpdateJerseyNumber(int? jerseyNumber)
    {
        if (JerseyNumber != jerseyNumber)
        {
            JerseyNumber = jerseyNumber;
            RequestedJerseyNumber = null;
        }
    }

    internal void AcknowledgeJerseyNumber() => RequestedJerseyNumber = null;

    internal void RecordGamePlayed() => GamesPlayed++;

    internal void RecordGoal() => Goals++;

    internal void RecordAssist() => Assists++;

    internal void RecordPenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes), "Penalty minutes cannot be negative.");
        PenaltyMinutes += minutes;
    }
}
