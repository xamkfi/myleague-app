using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;

namespace Domain.Entities.Floorball.Competitions;

/// <summary>
/// Lightweight season date projection for year aggregation.
/// </summary>
public record FloorballSeasonDateSummary(DateTime StartDate, DateTime EndDate, bool IsActive);
