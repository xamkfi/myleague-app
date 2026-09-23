using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Floorball;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Organization.PlayerLicences.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Domain.ValueObjects.Floorball;
using Domain.Entities.Common;

namespace Application.Features.Floorball.Players.DTOs
{
    /// <summary>
    /// Data Transfer Object for FloorballPlayer entity
    /// </summary>
    public record FloorballPlayerDto(
        Guid Id,
        Guid PersonId,
        PersonDto Person,
        bool IsActive,
        FloorballPosition Position,
        int CareerGoals,
        int CareerAssists,
        FloorballTeamNameDto? Team = null,
        IReadOnlyList<ActivePlayerLicenceDto>? ActiveLicences = null);
}
