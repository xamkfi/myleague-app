using Application.Commands.Floorball;
using Application.DTOs.Floorball;
using Domain.Entities.Floorball;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for FloorballCoach entity
/// </summary>
public static class FloorballCoachMapper
{
    /// <summary>
    /// Maps a FloorballCoach entity to a FloorballCoachDto
    /// </summary>
    /// <param name="coach">The coach entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when coach is null</exception>
    public static FloorballCoachDto ToDto(FloorballCoach coach)
    {
        if (coach == null)
            throw new ArgumentNullException(nameof(coach));

        return new FloorballCoachDto
        {
            Id = coach.Id,
            PersonId = coach.PersonId,
            IsActive = coach.IsActive,
            YearsOfExperience = coach.YearsOfExperience,
            CertificationLevel = coach.CertificationLevel,
            Specialization = coach.Specialization,
            CreatedAt = coach.CreatedAt.ToUniversalTime(),
            UpdatedAt = coach.UpdatedAt?.ToUniversalTime()
        };
    }

    /// <summary>
    /// Maps a collection of FloorballCoach entities to FloorballCoachDtos
    /// </summary>
    /// <param name="coaches">The coach entities to map</param>
    /// <returns>The mapped DTOs</returns>
    /// <exception cref="ArgumentNullException">Thrown when coaches is null</exception>
    public static IEnumerable<FloorballCoachDto> ToDtos(IEnumerable<FloorballCoach> coaches)
    {
        if (coaches == null)
            throw new ArgumentNullException(nameof(coaches));

        return coaches.Select(coach => ToDto(coach));
    }

    /// <summary>
    /// Creates a new FloorballCoach entity from a create command
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new coach entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public static FloorballCoach ToEntity(CreateFloorballCoachCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        return new FloorballCoach(
            command.PersonId,
            command.YearsOfExperience,
            command.CertificationLevel,
            command.Specialization);
    }

    /// <summary>
    /// Updates a FloorballCoach entity from an update command
    /// </summary>
    /// <param name="coach">The coach entity to update</param>
    /// <param name="command">The update command</param>
    /// <exception cref="ArgumentNullException">Thrown when coach or command is null</exception>
    public static void UpdateFromCommand(FloorballCoach coach, UpdateFloorballCoachCommand command)
    {
        if (coach == null)
            throw new ArgumentNullException(nameof(coach));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Note: The entity needs to expose methods for updating these properties
        // For now, assuming these methods exist or will be added
        coach.UpdateActiveStatus(command.IsActive);
        coach.UpdateExperience(command.YearsOfExperience);
        coach.UpdateCertification(command.CertificationLevel);
        coach.UpdateSpecialization(command.Specialization);
    }
} 