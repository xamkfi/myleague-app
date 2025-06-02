using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballMatchStatistics entity
/// </summary>
public static class FloorballMatchStatisticsMapper
{
    /// <summary>
    /// Maps a FloorballMatchStatistics entity to a FloorballMatchStatisticsDto
    /// </summary>
    /// <param name="statistics">The match statistics entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when statistics is null</exception>
    public static FloorballMatchStatisticsDto ToDto(FloorballMatchStatistics statistics)
    {
        if (statistics == null)
            throw new ArgumentNullException(nameof(statistics));

        return new FloorballMatchStatisticsDto
        {
            Id = statistics.Id,
            MatchId = statistics.MatchId,
            TeamId = statistics.TeamId,
            ShotsOnGoal = statistics.ShotsOnGoal,
            ShotsMissed = statistics.ShotsMissed,
            Saves = statistics.Saves,
            PowerPlays = statistics.PowerPlays,
            PowerPlayGoals = statistics.PowerPlayGoals,
            FaceoffsWon = statistics.FaceoffsWon,
            PenaltyMinutes = statistics.PenaltyMinutes,
            TimeoutsCalled = statistics.TimeoutsCalled,
            CreatedAt = statistics.CreatedAt.ToUniversalTime(),
            UpdatedAt = statistics.UpdatedAt?.ToUniversalTime()
        };
    }

    /// <summary>
    /// Maps a collection of FloorballMatchStatistics entities to FloorballMatchStatisticsDtos
    /// </summary>
    /// <param name="statisticsCollection">The match statistics entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when statisticsCollection is null</exception>
    public static IEnumerable<FloorballMatchStatisticsDto> ToDtos(IEnumerable<FloorballMatchStatistics> statisticsCollection)
    {
        if (statisticsCollection == null)
            throw new ArgumentNullException(nameof(statisticsCollection));

        return statisticsCollection.Select(stats => ToDto(stats));
    }

    /// <summary>
    /// Creates a new FloorballMatchStatistics entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new match statistics entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballMatchStatistics ToEntity(CreateFloorballMatchStatisticsCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballMatchStatistics
        {
            MatchId = command.MatchId,
            TeamId = command.TeamId,
            ShotsOnGoal = command.ShotsOnGoal,
            ShotsMissed = command.ShotsMissed,
            Saves = command.Saves,
            PowerPlays = command.PowerPlays,
            PowerPlayGoals = command.PowerPlayGoals,
            FaceoffsWon = command.FaceoffsWon,
            PenaltyMinutes = command.PenaltyMinutes,
            TimeoutsCalled = command.TimeoutsCalled,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates a FloorballMatchStatistics entity from an update command
    /// </summary>
    /// <param name="statistics">The match statistics entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when statistics or command is null</exception>
    public static void UpdateFromCommand(FloorballMatchStatistics statistics, UpdateFloorballMatchStatisticsCommand command)
    {
        if (statistics == null)
            throw new ArgumentNullException(nameof(statistics));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        statistics.ShotsOnGoal = command.ShotsOnGoal;
        statistics.ShotsMissed = command.ShotsMissed;
        statistics.Saves = command.Saves;
        statistics.PowerPlays = command.PowerPlays;
        statistics.PowerPlayGoals = command.PowerPlayGoals;
        statistics.FaceoffsWon = command.FaceoffsWon;
        statistics.PenaltyMinutes = command.PenaltyMinutes;
        statistics.TimeoutsCalled = command.TimeoutsCalled;
        statistics.UpdatedAt = DateTime.UtcNow;
    }
} 