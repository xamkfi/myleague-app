using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Query parameters for the teams listed in the all-time statistics team filter.
/// </summary>
public record GetAllTimeTeamsRequest
{
    /// <summary>
    /// Team category. Defaults to adult competitions.
    /// </summary>
    public TeamCategory TeamCategory { get; init; } = TeamCategory.Adult;

    /// <summary>
    /// Seasons, tournaments, or both. Defaults to seasons.
    /// </summary>
    public AllTimeCompetitionFilter CompetitionType { get; init; } = AllTimeCompetitionFilter.Season;
}
