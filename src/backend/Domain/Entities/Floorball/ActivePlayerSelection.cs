using Domain.Enums.Floorball;

namespace Domain.Entities.Floorball;

/// <summary>
/// Value object describing a single player's inclusion in a match's active field lineup, paired
/// with the per-match role they will play. Used as input to
/// <see cref="FloorballMatch.SetActiveRoster(System.Guid, System.Collections.Generic.IEnumerable{ActivePlayerSelection}, System.Guid?)"/>.
/// </summary>
/// <param name="PlayerId">The ID of the player being added to the lineup.</param>
/// <param name="Position">The field role the player will assume in this match.
/// Must be <see cref="FloorballPosition.Forward"/>, <see cref="FloorballPosition.Center"/> or
/// <see cref="FloorballPosition.Defender"/>; goalies are tracked separately on the match.</param>
public sealed record ActivePlayerSelection(Guid PlayerId, FloorballPosition Position);
