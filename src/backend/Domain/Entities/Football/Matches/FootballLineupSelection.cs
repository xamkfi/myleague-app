using Domain.Enums.Football;

namespace Domain.Entities.Football.Matches;

/// <summary>
/// Input for including a player in a match squad, with starting/bench status.
/// </summary>
public sealed record FootballLineupSelection(
    Guid PlayerId,
    FootballPosition Position,
    bool IsOnField);
