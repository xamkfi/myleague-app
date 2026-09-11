using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Floorball;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
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
        FloorballTeamNameDto? Team = null);
}
