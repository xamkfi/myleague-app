using System;

namespace Application.DTOs.Floorball;

/// <summary>
/// Data Transfer Object for a division within a floorball season
/// </summary>
/// <param name="DivisionId">The unique identifier of the division</param>
/// <param name="TeamCount">The number of teams in this division for this season</param>
public record FloorballSeasonDivisionDto(
    Guid DivisionId,
    int TeamCount);

