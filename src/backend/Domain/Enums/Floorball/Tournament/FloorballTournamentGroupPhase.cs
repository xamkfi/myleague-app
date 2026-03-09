namespace Domain.Enums.Floorball.Tournament;

/// <summary>
/// Identifies which phase of the tournament a group belongs to
/// </summary>
public enum FloorballTournamentGroupPhase
{
    /// <summary>
    /// Initial round-robin group stage (e.g. A-lohko, B-lohko)
    /// </summary>
    GroupStage = 0,

    /// <summary>
    /// Playoff/final group formed by advancing teams from the group stage
    /// </summary>
    Playoff = 1
}
