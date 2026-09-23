

using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
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
