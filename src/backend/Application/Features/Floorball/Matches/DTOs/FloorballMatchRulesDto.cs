namespace Application.Features.Floorball.Matches.DTOs
{
    /// <summary>
    /// Data Transfer Object for match rules configuration.
    /// </summary>
    /// <param name="NumberOfPeriods">Number of regular periods (e.g., 2 or 3)</param>
    /// <param name="PeriodDurationMinutes">Duration in minutes per regular period</param>
    /// <param name="AllowOvertime">Whether overtime is allowed</param>
    /// <param name="OvertimeDurationMinutes">Duration in minutes for overtime period</param>
    /// <param name="AllowShootout">Whether shootout is allowed after overtime</param>
    public record FloorballMatchRulesDto(
        int NumberOfPeriods,
        int PeriodDurationMinutes,
        bool AllowOvertime,
        int OvertimeDurationMinutes,
        bool AllowShootout);
}
