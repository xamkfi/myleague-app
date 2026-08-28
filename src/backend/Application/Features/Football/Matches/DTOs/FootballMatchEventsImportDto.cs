using System.Collections.Generic;

namespace Application.Features.Football.Matches.DTOs;

/// <summary>
/// Result of importing a batch of historical football match events in one request.
/// </summary>
public record FootballMatchEventsImportDto(
    FootballMatchDto Match,
    int GoalsRecorded,
    int CardsRecorded,
    IReadOnlyList<string> EventErrors);
