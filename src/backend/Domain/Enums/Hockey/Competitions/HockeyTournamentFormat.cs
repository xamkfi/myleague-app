namespace Domain.Enums.Hockey.Competitions;

/// <summary>
/// Represents the structural format of a hockey tournament.
/// </summary>
public enum HockeyTournamentFormat
{
    /// <summary>
    /// SingleGroup
    /// </summary>
    SingleGroup = 0,
    /// <summary>
    /// MultipleGroups
    /// </summary>
    MultipleGroups = 1,
    /// <summary>
    /// RoundRobin
    /// </summary>
    RoundRobin = 2,
    /// <summary>
    /// Knockout
    /// </summary>
    Knockout = 3,
    /// <summary>
    /// GroupsAndPlayoffs
    /// </summary>
    GroupsAndPlayoffs = 4,
    /// <summary>
    /// BestOfSeries
    /// </summary>
    BestOfSeries = 5,
    /// <summary>
    /// Custom
    /// </summary>
    Custom = 6
}

