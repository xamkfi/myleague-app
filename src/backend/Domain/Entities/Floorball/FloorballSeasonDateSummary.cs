namespace Domain.Entities.Floorball;

/// <summary>
/// Lightweight season date projection for year aggregation.
/// </summary>
public record FloorballSeasonDateSummary(DateTime StartDate, DateTime EndDate, bool IsActive);
