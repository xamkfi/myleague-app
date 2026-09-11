using System.Collections.Generic;

namespace Application.Features.Floorball.Matches.DTOs;

/// <summary>
/// Result of importing a batch of historical floorball match events in one request.
/// </summary>
public record FloorballMatchEventsImportDto(
    FloorballMatchDto Match,
    int GoalsRecorded,
    int PenaltiesRecorded,
    IReadOnlyList<string> EventErrors);
