namespace Domain.Common;

/// <summary>
/// Open team-roster membership that represents a sport-specific player licence.
/// </summary>
public sealed record PlayerLicenceRow(
    Guid TeamId,
    string TeamName,
    Guid? CompetitionId,
    string? CompetitionName,
    bool IsActive);
