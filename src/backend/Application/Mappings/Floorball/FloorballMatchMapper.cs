using Application.Commands.Floorball.Match;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
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
    public static FloorballMatchDto ToDto(FloorballMatch match, Club? homeClub, Club? awayClub)
    {
        return ToDto(match, homeClub!, awayClub!, new Dictionary<Guid, Person>());
    }

    /// <summary>
    /// Maps a FloorballMatch entity to a FloorballMatchDto with person lookup for player names
    /// </summary>
    /// <param name="match">The match entity to map</param>
    /// <param name="playerPersonLookup">Dictionary mapping player IDs to their person data</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when match is null</exception>
    public static FloorballMatchDto ToDto(FloorballMatch match, Club? homeClub, Club? awayClub, Dictionary<Guid, Person> playerPersonLookup)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        playerPersonLookup ??= new Dictionary<Guid, Person>();

        // Map officials from the match entity
        List<Guid> officials = match.Officials.Select(referee => referee.Id).ToList();

        // Map period scores from the match entity, ordered by period number
        Dictionary<int, PeriodScoreDto> periodScores = match.PeriodScores
            .OrderBy(ps => ps.PeriodNumber)
            .ToDictionary(
                ps => ps.PeriodNumber,
                ps => new PeriodScoreDto(ps.HomeScore, ps.AwayScore)
            );

        // Map goal events with player names
        List<FloorballGoalEventDto> goalEvents = match.GoalEvents
            .Select(g => new FloorballGoalEventDto(
                g.TeamId,
                g.ScoringPlayerId ?? Guid.Empty,
                g.AssistingPlayerId,
                g.SecondaryAssistingPlayerId,
                g.PeriodNumber,
                g.TimeInSeconds,
                match.WentToOvertime,
                match.WentToShootout,
                GetPlayerName(g.ScoringPlayerId, playerPersonLookup),
                GetPlayerName(g.AssistingPlayerId, playerPersonLookup),
                GetPlayerName(g.SecondaryAssistingPlayerId, playerPersonLookup)))
            .ToList();

        // Map penalty events with player names
        List<FloorballPenaltyEventDto> penaltyEvents = match.PenaltyEvents
            .Select(p => new FloorballPenaltyEventDto(
                p.TeamId,
                p.PlayerId,
                p.PenaltyType,
                p.DurationInMinutes,
                p.PeriodNumber,
                p.TimeInSeconds,
                p.Description ?? string.Empty,
                GetPlayerName(p.PlayerId, playerPersonLookup)))
            .ToList();

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
            goalEvents,
            penaltyEvents,
            homeClub,
            awayClub);
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

        return matches.Select(match => ToDto(match, null, null));
    }

    /// <summary>
    /// Maps an EventSourcedFloorballMatch entity to a FloorballMatchDto
    /// </summary>
    /// <param name="match">The event-sourced match entity to map</param>
    /// <param name="homeTeamName">The home team name (placeholder until team lookups are implemented)</param>
    /// <param name="awayTeamName">The away team name (placeholder until team lookups are implemented)</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when match is null</exception>
    public static FloorballMatchDto ToDto(EventSourcedFloorballMatch match, string homeTeamName = "Home Team", string awayTeamName = "Away Team")
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        // Convert period scores from tuple format to PeriodScoreDto format
        Dictionary<int, PeriodScoreDto> periodScores = ConvertPeriodScores(match.PeriodScores);

        return new FloorballMatchDto(
            match.Id,
            match.SeasonId,
            match.HomeTeamId,
            homeTeamName,
            match.AwayTeamId,
            awayTeamName,
            match.ScheduledDateTime,
            match.Venue,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.WentToOvertime,
            match.WentToShootout,
            periodScores,
            match.OfficialIds,
            new List<FloorballGoalEventDto>(), // TODO: Map goal events when needed
            new List<FloorballPenaltyEventDto>(), // TODO: Map penalty events when needed
            null,
            null
        );
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

    /// <summary>
    /// Converts period scores from tuple format to PeriodScoreDto format
    /// </summary>
    /// <param name="periodScores">The period scores in tuple format</param>
    /// <returns>The period scores in PeriodScoreDto format</returns>
    public static Dictionary<int, PeriodScoreDto> ConvertPeriodScores(IReadOnlyDictionary<int, (int HomeScore, int AwayScore)> periodScores)
    {
        if (periodScores == null)
            return new Dictionary<int, PeriodScoreDto>();

        return periodScores.ToDictionary(
            kvp => kvp.Key,
            kvp => new PeriodScoreDto(kvp.Value.HomeScore, kvp.Value.AwayScore)
        );
    }

    /// <summary>
    /// Gets the player name from the person lookup dictionary
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="playerPersonLookup">Dictionary mapping player IDs to their person data</param>
    /// <returns>The player's full name or "Unknown Player" if not found</returns>
    private static string GetPlayerName(Guid? playerId, Dictionary<Guid, Person> playerPersonLookup)
    {
        if (!playerId.HasValue || !playerPersonLookup.TryGetValue(playerId.Value, out Person? person))
        {
            return "Unknown Player";
        }

        return $"{person.FirstName} {person.LastName}";
    }
} 
