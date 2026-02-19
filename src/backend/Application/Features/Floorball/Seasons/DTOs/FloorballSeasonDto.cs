using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Floorball;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Data Transfer Object for FloorballSeason entity
    /// </summary>
    /// <param name="Id">The unique identifier of the season</param>
    /// <param name="Name">The name of the season (e.g., "2023-2024")</param>
    /// <param name="StartDate">The start date of the season</param>
    /// <param name="EndDate">The end date of the season</param>
    /// <param name="IsActive">Whether the season is currently active</param>
    /// <param name="IsCompleted">Whether the season is completed</param>
    /// <param name="SeasonDivisions">List of divisions associated with this season, including team counts</param>
    /// <param name="Teams">List of teams participating in this season</param>
    /// <param name="Matches">List of matches scheduled for this season</param>
    /// <param name="MatchRules">Match rules configuration for this season</param>
    public record FloorballSeasonDto(
        Guid Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        bool IsActive,
        bool IsCompleted,
        IReadOnlyCollection<FloorballSeasonDivisionDto> SeasonDivisions,
        IReadOnlyCollection<FloorballTeamDto> Teams,
        IReadOnlyCollection<FloorballMatchDto> Matches,
        FloorballMatchRulesDto MatchRules);
}
