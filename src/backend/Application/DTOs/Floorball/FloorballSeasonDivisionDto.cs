using System;
using System.Collections.Generic;

namespace Application.DTOs.Floorball;

/// <summary>
/// Data Transfer Object for a division within a floorball season
/// </summary>
/// <param name="DivisionId">The unique identifier of the division</param>
/// <param name="TeamCount">The number of teams in this division for this season</param>
/// <param name="TeamIds">The IDs of teams in this division for this season</param>
public record FloorballSeasonDivisionDto(
    Guid DivisionId,
    int TeamCount,
    IReadOnlyCollection<Guid> TeamIds);

