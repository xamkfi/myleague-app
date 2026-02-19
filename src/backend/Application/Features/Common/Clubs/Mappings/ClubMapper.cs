using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Common.Clubs.Mappings;

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

        DateTime? foundingDateUtc = null;
        if (command.FoundingDate.HasValue)
        {
            DateTime foundingDate = command.FoundingDate.Value;
            // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
            foundingDateUtc = foundingDate.Kind switch
            {
                DateTimeKind.Utc => foundingDate,
                DateTimeKind.Local => foundingDate.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(foundingDate, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(foundingDate, DateTimeKind.Utc)
            };
        }

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

        // Update founding date with UTC conversion (only if provided)
        if (command.FoundingDate.HasValue)
        {
            DateTime foundingDateUtc = command.FoundingDate.Value.Kind switch
            {
                DateTimeKind.Utc => command.FoundingDate.Value,
                DateTimeKind.Local => command.FoundingDate.Value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(command.FoundingDate.Value, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(command.FoundingDate.Value, DateTimeKind.Utc)
            };
            club.UpdateFoundingDate(foundingDateUtc);
        }

        // Update online presence
        Uri? websiteUri = !string.IsNullOrEmpty(command.WebsiteUrl) ? new Uri(command.WebsiteUrl) : null;
        Uri? logoUri = !string.IsNullOrEmpty(command.LogoUrl) ? new Uri(command.LogoUrl) : null;
        club.UpdateOnlinePresence(websiteUri, logoUri, command.ContactEmail);
    }
} 
