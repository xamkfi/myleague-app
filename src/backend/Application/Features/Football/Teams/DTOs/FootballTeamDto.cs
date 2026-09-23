using System;
using System.Collections.Generic;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Enums.Common;
using Domain.Enums.Football;

namespace Application.Features.Football.Teams.DTOs
{
    /// <summary>
    /// Data Transfer Object for FootballTeam entity
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
    public record FootballTeamDto(
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
        IReadOnlyCollection<FootballTeamPlayerDto> Roster,
        TeamCategory TeamCategory);
}
