namespace Domain.Enums.Floorball;

/// <summary>
/// Identifies which concrete competition type a floorball match belongs to.
/// Used as a filter when querying matches across the TPH FloorballCompetition hierarchy.
/// </summary>
public enum FloorballCompetitionType
{
    Season = 0,
    Tournament = 1
}
