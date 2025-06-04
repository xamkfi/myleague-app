using Application.Commands.Clubs;
using Application.DTOs.Common;
using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Common;

/// <summary>
/// Mapper class for Club entity and related DTOs
/// </summary>
public static class ClubMapper
{
    /// <summary>
    /// Maps a Club entity to a ClubDto
    /// </summary>
    /// <param name="club">The Club entity to map</param>
    /// <returns>A ClubDto representing the Club entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if club is null</exception>
    public static ClubDto ToDto(Club club)
    {
        if (club == null)
            throw new ArgumentNullException(nameof(club));

        return new ClubDto(
            club.Id,
            club.Name,
            club.FoundingDate,
            club.City,
            club.Country,
            club.WebsiteUrl?.ToString() ?? string.Empty,
            club.LogoUrl?.ToString() ?? string.Empty,
            club.ContactEmail
        );
    }

    /// <summary>
    /// Maps a collection of Club entities to a collection of ClubDtos
    /// </summary>
    /// <param name="clubs">The collection of Club entities to map</param>
    /// <returns>A collection of ClubDtos</returns>
    /// <exception cref="ArgumentNullException">Thrown if clubs is null</exception>
    public static IEnumerable<ClubDto> ToDtos(IEnumerable<Club> clubs)
    {
        if (clubs == null)
            throw new ArgumentNullException(nameof(clubs));

        return clubs.Select(club => ToDto(club));
    }

    /// <summary>
    /// Maps a CreateClubCommand to a Club entity
    /// </summary>
    /// <param name="command">The CreateClubCommand to map</param>
    /// <returns>A new Club entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
    public static Club ToEntity(CreateClubCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        Uri? websiteUri = !string.IsNullOrEmpty(command.WebsiteUrl) ? new Uri(command.WebsiteUrl) : null;
        Uri? logoUri = !string.IsNullOrEmpty(command.LogoUrl) ? new Uri(command.LogoUrl) : null;

        // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
        DateTime foundingDateUtc = command.FoundingDate.Kind switch
        {
            DateTimeKind.Utc => command.FoundingDate,
            DateTimeKind.Local => command.FoundingDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.FoundingDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.FoundingDate, DateTimeKind.Utc)
        };

        return new Club(
            command.Name,
            command.City,
            command.Country,
            foundingDateUtc,
            websiteUri,
            logoUri,
            command.ContactEmail
        );
    }

    /// <summary>
    /// Updates a Club entity with values from an UpdateClubCommand
    /// </summary>
    /// <param name="club">The Club entity to update</param>
    /// <param name="command">The UpdateClubCommand containing updated values</param>
    /// <exception cref="ArgumentNullException">Thrown if club or command is null</exception>
    public static void UpdateFromCommand(Club club, UpdateClubCommand command)
    {
        if (club == null)
            throw new ArgumentNullException(nameof(club));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Update basic info
        club.UpdateBasicInfo(command.Name, command.City, command.Country);

        // Update founding date with UTC conversion
        DateTime foundingDateUtc = command.FoundingDate.Kind switch
        {
            DateTimeKind.Utc => command.FoundingDate,
            DateTimeKind.Local => command.FoundingDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(command.FoundingDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(command.FoundingDate, DateTimeKind.Utc)
        };
        club.UpdateFoundingDate(foundingDateUtc);

        // Update online presence
        Uri? websiteUri = !string.IsNullOrEmpty(command.WebsiteUrl) ? new Uri(command.WebsiteUrl) : null;
        Uri? logoUri = !string.IsNullOrEmpty(command.LogoUrl) ? new Uri(command.LogoUrl) : null;
        club.UpdateOnlinePresence(websiteUri, logoUri, command.ContactEmail);
    }
} 
