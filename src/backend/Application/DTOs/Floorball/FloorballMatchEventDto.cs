using System;
using Domain.Enums.Floorball;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Data Transfer Object for a goal scored in a floorball match
    /// </summary>
    /// <param name="TeamId">The ID of the team that scored</param>
    /// <param name="PlayerId">The ID of the player who scored</param>
    /// <param name="AssisterId">The ID of the player who assisted (optional)</param>
    /// <param name="PeriodNumber">The period number when the goal was scored</param>
    /// <param name="TimeInSeconds">The time in seconds when the goal was scored</param>
    /// <param name="WasInOvertime">Whether the goal was scored in overtime</param>
    /// <param name="WasInShootout">Whether the goal was scored in shootout</param>
    public record FloorballGoalEventDto(
        Guid TeamId,
        Guid PlayerId,
        Guid? AssisterId,
        Guid? SecondaryAssisterId,
        int PeriodNumber,
        int TimeInSeconds,
        bool WasInOvertime,
        bool WasInShootout);

    /// <summary>
    /// Data Transfer Object for a penalty in a floorball match
    /// </summary>
    /// <param name="TeamId">The ID of the team that received the penalty</param>
    /// <param name="PlayerId">The ID of the player who received the penalty (optional for team penalties)</param>
    /// <param name="PenaltyType">The type of penalty</param>
    /// <param name="Minutes">The duration of the penalty in minutes</param>
    /// <param name="PeriodNumber">The period number when the penalty was given</param>
    /// <param name="TimeInSeconds">The time in seconds when the penalty was given</param>
    /// <param name="Description">Description of the penalty</param>
    public record FloorballPenaltyEventDto(
        Guid TeamId,
        Guid? PlayerId,
        FloorballPenaltyType PenaltyType,
        int Minutes,
        int PeriodNumber,
        int TimeInSeconds,
        string Description);
} 
