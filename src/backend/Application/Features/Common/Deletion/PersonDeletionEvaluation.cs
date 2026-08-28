namespace Application.Features.Common.Deletion;

/// <summary>
/// Result of evaluating whether a person can be hard-deleted.
/// Unused sport profiles may be removed in the same operation.
/// </summary>
public sealed class PersonDeletionEvaluation
{
    public string? BlockReason { get; init; }

    public Guid? UnusedFloorballPlayerId { get; init; }

    public Guid? UnusedFootballPlayerId { get; init; }

    public Guid? UnusedHockeyPlayerId { get; init; }

    public Guid? UnusedFloorballRefereeId { get; init; }

    public Guid? UnusedFootballRefereeId { get; init; }

    public Guid? UnusedHockeyOfficialId { get; init; }

    public IReadOnlyCollection<Guid> FloorballTeamManagerIds { get; init; } = Array.Empty<Guid>();

    public IReadOnlyCollection<Guid> FootballTeamManagerIds { get; init; } = Array.Empty<Guid>();

    public bool IsBlocked => !string.IsNullOrEmpty(BlockReason);
}
