using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballTeamStatistics entity
/// </summary>
public static class FloorballTeamStatisticsMapper
{
    /// <summary>
    /// Maps a FloorballTeamStatistics entity to a FloorballTeamStatisticsDto
    /// </summary>
    /// <param name="statistics">The team statistics entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when statistics is null</exception>
    public static FloorballTeamStatisticsDto ToDto(FloorballTeamStatistics statistics)
    {
        if (statistics == null)
            throw new ArgumentNullException(nameof(statistics));

        return new FloorballTeamStatisticsDto
        {
            Id = statistics.Id,
            TeamId = statistics.TeamId,
            SeasonId = statistics.SeasonId,
            GamesPlayed = statistics.GamesPlayed,
            Wins = statistics.Wins,
            Losses = statistics.Losses,
            Draws = statistics.Draws,
            GoalsFor = statistics.GoalsFor,
            GoalsAgainst = statistics.GoalsAgainst,
            Points = statistics.Points,
            PowerPlayPercentage = statistics.PowerPlayPercentage,
            PenaltyKillPercentage = statistics.PenaltyKillPercentage,
            CreatedAt = statistics.CreatedAt.ToUniversalTime(),
            UpdatedAt = statistics.UpdatedAt?.ToUniversalTime()
        };
    }

    /// <summary>
    /// Maps a collection of FloorballTeamStatistics entities to FloorballTeamStatisticsDtos
    /// </summary>
    /// <param name="statisticsCollection">The team statistics entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when statisticsCollection is null</exception>
    public static IEnumerable<FloorballTeamStatisticsDto> ToDtos(IEnumerable<FloorballTeamStatistics> statisticsCollection)
    {
        if (statisticsCollection == null)
            throw new ArgumentNullException(nameof(statisticsCollection));

        return statisticsCollection.Select(stats => ToDto(stats));
    }

    /// <summary>
    /// Creates a new FloorballTeamStatistics entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new team statistics entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballTeamStatistics ToEntity(CreateFloorballTeamStatisticsCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballTeamStatistics
        {
            TeamId = command.TeamId,
            SeasonId = command.SeasonId,
            GamesPlayed = command.GamesPlayed,
            Wins = command.Wins,
            Losses = command.Losses,
            Draws = command.Draws,
            GoalsFor = command.GoalsFor,
            GoalsAgainst = command.GoalsAgainst,
            Points = command.Points,
            PowerPlayPercentage = command.PowerPlayPercentage,
            PenaltyKillPercentage = command.PenaltyKillPercentage,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates a FloorballTeamStatistics entity from an update command
    /// </summary>
    /// <param name="statistics">The team statistics entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when statistics or command is null</exception>
    public static void UpdateFromCommand(FloorballTeamStatistics statistics, UpdateFloorballTeamStatisticsCommand command)
    {
        if (statistics == null)
            throw new ArgumentNullException(nameof(statistics));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        statistics.GamesPlayed = command.GamesPlayed;
        statistics.Wins = command.Wins;
        statistics.Losses = command.Losses;
        statistics.Draws = command.Draws;
        statistics.GoalsFor = command.GoalsFor;
        statistics.GoalsAgainst = command.GoalsAgainst;
        statistics.Points = command.Points;
        statistics.PowerPlayPercentage = command.PowerPlayPercentage;
        statistics.PenaltyKillPercentage = command.PenaltyKillPercentage;
        statistics.UpdatedAt = DateTime.UtcNow;
    }
} 