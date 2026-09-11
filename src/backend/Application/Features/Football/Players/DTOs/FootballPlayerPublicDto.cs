

using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Football.Teams.DTOs;
using Domain.Enums.Football;

namespace Application.Features.Football.Players.DTOs
{
    public record FootballPlayerPublicDto(
        Guid Id,
        Guid PersonId,
        PersonPublicDto Person,
        bool IsActive,
        FootballPosition Position,
        int CareerGoals,
        int CareerAssists,
        FootballTeamNameDto? Team = null);
}
