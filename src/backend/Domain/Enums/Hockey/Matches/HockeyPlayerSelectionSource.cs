namespace Domain.Enums.Hockey.Matches;

/// <summary>
/// Identifies how a player was selected for a match roster.
/// </summary>
public enum HockeyPlayerSelectionSource
{
    /// <summary>
    /// PreSelected
    /// </summary>
    PreSelected = 0,
    /// <summary>
    /// SelectedAtGameStart
    /// </summary>
    SelectedAtGameStart = 1,
    /// <summary>
    /// CopiedFromTeamRoster
    /// </summary>
    CopiedFromTeamRoster = 2,
    /// <summary>
    /// CopiedFromDefaultLines
    /// </summary>
    CopiedFromDefaultLines = 3,
    /// <summary>
    /// Manual
    /// </summary>
    Manual = 4
}

