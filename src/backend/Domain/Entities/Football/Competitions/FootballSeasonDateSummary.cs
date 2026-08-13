namespace Domain.Entities.Football.Competitions;

/// <summary>
/// Lightweight season date projection for year aggregation.
/// </summary>
public record FootballSeasonDateSummary(DateTime StartDate, DateTime EndDate, bool IsActive);
