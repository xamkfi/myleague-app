using System.Collections.Generic;

namespace Application.Features.Hockey.Matches.DTOs;

/// <summary>
/// Result of importing a batch of historical hockey match events in one request.
/// </summary>
public record HockeyMatchEventsImportDto(
    HockeyMatchDto Match,
    int GoalsRecorded,
    int PenaltiesRecorded,
    IReadOnlyList<string> EventErrors);
