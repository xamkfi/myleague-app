using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Football;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Football.Teams.DTOs;
using Domain.ValueObjects.Football;
using Domain.Entities.Common;

namespace Application.Features.Football.Players.DTOs
{
    /// <summary>
    /// Data Transfer Object for FootballPlayer entity
    /// </summary>
    public record FootballPlayerDto(
        Guid Id,
        Guid PersonId,
        PersonDto Person,
        bool IsActive,
        FootballPosition Position,
        int CareerGoals,
        int CareerAssists,
        FootballTeamNameDto? Team = null);
}
