using Application.Commands.Floorball;
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

        return new FloorballMatchDto
        {
            Id = match.Id,
            SeasonId = match.SeasonId,
            HomeTeamId = match.HomeTeamId,
            AwayTeamId = match.AwayTeamId,
            HomeTeamScore = match.HomeTeamScore,
            AwayTeamScore = match.AwayTeamScore,
            MatchDate = match.MatchDate.ToUniversalTime(),
            Venue = match.Venue,
            Status = match.Status,
            RefereeNotes = match.RefereeNotes,
            CreatedAt = match.CreatedAt.ToUniversalTime(),
            UpdatedAt = match.UpdatedAt?.ToUniversalTime(),
            IsActive = match.IsActive
        };
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
    public static FloorballMatch ToEntity(CreateFloorballMatchCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballMatch
        {
            SeasonId = command.SeasonId,
            HomeTeamId = command.HomeTeamId,
            AwayTeamId = command.AwayTeamId,
            HomeTeamScore = command.HomeTeamScore,
            AwayTeamScore = command.AwayTeamScore,
            MatchDate = command.MatchDate.ToUniversalTime(),
            Venue = command.Venue,
            Status = command.Status,
            RefereeNotes = command.RefereeNotes,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
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

        match.HomeTeamId = command.HomeTeamId;
        match.AwayTeamId = command.AwayTeamId;
        match.HomeTeamScore = command.HomeTeamScore;
        match.AwayTeamScore = command.AwayTeamScore;
        match.MatchDate = command.MatchDate.ToUniversalTime();
        match.Venue = command.Venue;
        match.Status = command.Status;
        match.RefereeNotes = command.RefereeNotes;
        match.IsActive = command.IsActive;
        match.UpdatedAt = DateTime.UtcNow;
    }
} 