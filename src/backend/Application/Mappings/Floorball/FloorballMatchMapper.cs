using Application.Commands.Floorball.Match;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballMatch entity
/// </summary>
public static class FloorballMatchMapper
{
    /// <summary>
    /// Maps a FloorballMatch entity to a FloorballMatchDto
    /// </summary>
    /// <param name="match">The match entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when match is null</exception>
    public static FloorballMatchDto ToDto(FloorballMatch match)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        // TODO: In a complete implementation, team names should be loaded from TeamRepository
        // TODO: Period scores, officials, and events should be loaded from respective repositories
        // For now, providing placeholder values to resolve compilation error
        return new FloorballMatchDto(
            match.Id,
            match.SeasonId,
            match.HomeTeamId,
            "Home Team", // HomeTeamName - placeholder
            match.AwayTeamId,
            "Away Team", // AwayTeamName - placeholder
            match.ScheduledDateTime.ToUniversalTime(),
            match.Venue,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            false, // WentToOvertime - placeholder
            false, // WentToShootout - placeholder
            new Dictionary<int, (int HomeScore, int AwayScore)>(), // Empty period scores
            new List<Guid>(), // Empty officials list
            new List<FloorballGoalEventDto>(), // Empty goal events
            new List<FloorballPenaltyEventDto>() // Empty penalty events
        );
    }

    /// <summary>
    /// Maps a collection of FloorballMatch entities to FloorballMatchDtos
    /// </summary>
    /// <param name="matches">The match entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when matches is null</exception>
    public static IEnumerable<FloorballMatchDto> ToDtos(IEnumerable<FloorballMatch> matches)
    {
        if (matches == null)
            throw new ArgumentNullException(nameof(matches));

        return matches.Select(match => ToDto(match));
    }

    /// <summary>
    /// Creates a new FloorballMatch entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new match entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="NotSupportedException">Thrown because FloorballMatch creation requires loaded entities</exception>
    public static FloorballMatch ToEntity(CreateFloorballMatchCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // FloorballMatch constructor requires loaded FloorballSeason and FloorballTeam entities,
        // but the command only contains IDs. This mapping should be handled in the handler
        // where repositories are available to load the necessary entities.
        throw new NotSupportedException(
            "FloorballMatch creation from command requires loaded entities (Season, HomeTeam, AwayTeam). " +
            "This should be handled in the handler layer where repositories are available.");
    }

    /// <summary>
    /// Updates a FloorballMatch entity from an update command
    /// </summary>
    /// <param name="match">The match entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when match or command is null</exception>
    public static void UpdateFromCommand(FloorballMatch match, UpdateFloorballMatchCommand command)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Use the domain entity's Reschedule method to update scheduled date/time and venue
        // This properly handles business rules and domain events
        match.Reschedule(command.ScheduledDateTime, command.Venue);
    }
} 
