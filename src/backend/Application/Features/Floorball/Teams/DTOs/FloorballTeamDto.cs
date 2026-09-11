using System;
using System.Collections.Generic;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Teams.DTOs
{
    /// <summary>
    /// Data Transfer Object for FloorballTeam entity
    /// </summary>
    /// <param name="Id">The unique identifier of the team</param>
    /// <param name="Name">The name of the team</param>
    /// <param name="ShortName">The short name of the team</param>
    /// <param name="Division">The division level of the team</param>
    /// <param name="Club">The club this team belongs to</param>
    /// <param name="HomeArena">The team's home arena</param>
    /// <param name="PrimaryJerseyColor">The team's primary jersey color</param>
    /// <param name="SecondaryJerseyColor">The team's secondary jersey color</param>
    /// <param name="LogoUrl">The team's logo URL</param>
    /// <param name="HasActiveMembers">Whether the team has any active members</param>
    /// <param name="Roster">The team's roster of players</param>
    /// <param name="TeamCategory">Audience / age-group category</param>
    public record FloorballTeamDto(
        Guid Id,
        string Name,
        string ShortName,
        Guid? DivisionId,
        ClubDto Club,
        string HomeArena,
        string PrimaryJerseyColor,
        string SecondaryJerseyColor,
        string? LogoUrl,
        bool HasActiveMembers,
        IReadOnlyCollection<FloorballTeamPlayerDto> Roster,
        TeamCategory TeamCategory);
}
