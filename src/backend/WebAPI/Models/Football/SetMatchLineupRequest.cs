using Domain.Enums.Football;

namespace WebAPI.Models.Football;

/// <summary>
/// Request model for setting a team's match lineup
/// </summary>
public class SetMatchLineupRequest
{
    /// <summary>
    /// Players included in the lineup
    /// </summary>
    public List<LineupPlayerRequest> Players { get; set; } = new();
}

/// <summary>
/// One player entry in a match lineup
/// </summary>
public class LineupPlayerRequest
{
    /// <summary>
    /// Player identifier
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Assigned position
    /// </summary>
    public FootballPosition Position { get; set; }

    /// <summary>
    /// Whether the player starts on the field
    /// </summary>
    public bool IsOnField { get; set; }
}
