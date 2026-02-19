

using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Players.DTOs
{
    public record FloorballPlayerPublicDto(
        Guid Id,
        Guid PersonId,
        PersonPublicDto Person,
        bool IsActive,
        FloorballPosition Position,
        int CareerGoals,
        int CareerAssists,
        FloorballTeamNameDto? Team = null);
}
