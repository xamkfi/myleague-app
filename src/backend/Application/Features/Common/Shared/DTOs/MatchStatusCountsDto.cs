namespace Application.Features.Common.Shared.DTOs;

/// <summary>
/// Match counts per status for the admin match list tabs.
/// </summary>
public record MatchStatusCountsDto(
    int Total,
    int Scheduled,
    int Postponed,
    int InProgress,
    int Completed,
    int Cancelled);
