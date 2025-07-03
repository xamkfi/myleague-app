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

        // Map officials from the match entity
        var officials = match.Officials.Select(referee => referee.Id).ToList();
        
        // Map period scores from the match entity
        var periodScores = match.PeriodScores.ToDictionary(
            ps => ps.PeriodNumber,
            ps => (ps.HomeScore, ps.AwayScore)
        );

        return new FloorballMatchDto(
            match.Id,
            match.SeasonId,
            match.HomeTeamId,
            match.HomeTeam.Name,
            match.AwayTeamId,
            match.AwayTeam.Name,
            match.ScheduledDateTime.ToUniversalTime(),
            match.Venue,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.WentToOvertime,
            match.WentToShootout,
            periodScores,
            officials,
            new List<FloorballGoalEventDto>(), // TODO: Map goal events when needed
            new List<FloorballPenaltyEventDto>() // TODO: Map penalty events when needed
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
    /// <param name="season">The season entity</param>
    /// <param name="homeTeam">The home team entity</param>
    /// <param name="awayTeam">The away team entity</param>
    /// <param name="referee">The referee entity (optional)</param>
    /// <returns>The new match entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    /// <exception cref="NotSupportedException">Thrown because FloorballMatch creation requires loaded entities</exception>
    public static FloorballMatch ToEntity(CreateFloorballMatchCommand command, FloorballSeason season, FloorballTeam homeTeam, FloorballTeam awayTeam, FloorballReferee? referee = null)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
        DateTime scheduledDateTimeUtc = command.ScheduledDateTime.Kind switch
        {
            DateTimeKind.Utc => command.ScheduledDateTime,
            DateTimeKind.Local => command.ScheduledDateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.ScheduledDateTime, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.ScheduledDateTime, DateTimeKind.Utc)
        };

        FloorballMatch match = new FloorballMatch(
            season,
            homeTeam,
            awayTeam,
            scheduledDateTimeUtc,
            command.Venue
            );

        // Add referee if provided
        if (referee != null)
        {
            match.AddOfficial(referee);
        }

        return match;
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
