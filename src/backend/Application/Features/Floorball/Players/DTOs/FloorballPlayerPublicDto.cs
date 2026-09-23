

using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
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
