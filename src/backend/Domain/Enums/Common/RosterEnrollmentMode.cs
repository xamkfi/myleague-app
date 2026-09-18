namespace Domain.Enums.Common;

/// <summary>
/// How a team's roster is initialized when the team joins a competition.
/// </summary>
public enum RosterEnrollmentMode
{
    /// <summary>
    /// Copy the latest existing competition roster (or the base roster when none exists).
    /// </summary>
    CopyLatest = 0,

    /// <summary>
    /// Join without copying players. Roster is filled later.
    /// </summary>
    Empty = 1,
}
