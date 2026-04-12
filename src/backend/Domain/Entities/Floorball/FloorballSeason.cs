using Domain.ValueObjects.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball league season (e.g., "2023-2024")
/// </summary>
public class FloorballSeason : FloorballCompetition
{
    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballSeason() : base() { }

    /// <summary>
    /// Initializes a new instance of the FloorballSeason class
    /// </summary>
    /// <param name="name">The name of the season</param>
    /// <param name="startDate">The start date of the season</param>
    /// <param name="endDate">The end date of the season</param>
    /// <param name="matchRules">Optional match rules configuration. If null, defaults are used.</param>
    public FloorballSeason(string name, DateTime startDate, DateTime endDate, FloorballMatchRules? matchRules = null)
        : base(name, startDate, endDate, matchRules) { }
}
