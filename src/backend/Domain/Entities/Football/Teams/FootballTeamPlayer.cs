using Domain.Enums.Football;

namespace Domain.Entities.Football.Teams;

/// <summary>
/// A player's membership in a football team roster.
/// </summary>
public class FootballTeamPlayer : BaseEntity
{
    public Guid TeamId { get; private set; }
    public Guid PlayerId { get; private set; }
    public FootballPosition Position { get; private set; }
    public bool IsActive { get; private set; }
    public int? JerseyNumber { get; private set; }
    public int? RequestedJerseyNumber { get; private set; }
    public bool HasJerseyNumberSubstituted =>
        RequestedJerseyNumber.HasValue && RequestedJerseyNumber != JerseyNumber;
    public int GamesPlayed { get; private set; }
    public int Goals { get; private set; }
    public int Assists { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }

    private FootballTeamPlayer()
    {
        IsActive = true;
    }

    public FootballTeamPlayer(Guid teamId, Guid playerId, FootballPosition position, int? jerseyNumber = null)
        : this(teamId, playerId, position, jerseyNumber, requestedJerseyNumber: null)
    {
    }

    public FootballTeamPlayer(
        Guid teamId,
        Guid playerId,
        FootballPosition position,
        int? jerseyNumber,
        int? requestedJerseyNumber)
    {
        TeamId = teamId;
        PlayerId = playerId;
        Position = position;
        JerseyNumber = jerseyNumber;
        RequestedJerseyNumber = requestedJerseyNumber.HasValue && requestedJerseyNumber != jerseyNumber
            ? requestedJerseyNumber
            : null;
        IsActive = true;
    }

    public void UpdatePosition(FootballPosition newPosition) => Position = newPosition;
    public void SetActiveStatus(bool isActive) => IsActive = isActive;

    public void UpdateJerseyNumber(int? jerseyNumber)
    {
        if (JerseyNumber != jerseyNumber)
        {
            JerseyNumber = jerseyNumber;
            RequestedJerseyNumber = null;
        }
    }

    public void AcknowledgeJerseyNumber() => RequestedJerseyNumber = null;
    public void RecordGamePlayed() => GamesPlayed++;
    public void RecordGoal() => Goals++;
    public void RecordAssist() => Assists++;
    public void RecordYellowCard() => YellowCards++;
    public void RecordRedCard() => RedCards++;

    public void RemoveGoal()
    {
        if (Goals > 0)
            Goals--;
    }

    public void RemoveAssist()
    {
        if (Assists > 0)
            Assists--;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FootballTeamPlayer other)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return TeamId == other.TeamId && PlayerId == other.PlayerId;
    }

    public override int GetHashCode() => HashCode.Combine(TeamId, PlayerId);

    public static bool operator ==(FootballTeamPlayer? left, FootballTeamPlayer? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(FootballTeamPlayer? left, FootballTeamPlayer? right) => !(left == right);
}
