using Domain.Enums.Football;

namespace WebAPI.Models.Football;

public class SetMatchLineupRequest
{
    public List<LineupPlayerRequest> Players { get; set; } = new();
}

public class LineupPlayerRequest
{
    public Guid PlayerId { get; set; }
    public FootballPosition Position { get; set; }
    public bool IsOnField { get; set; }
}
