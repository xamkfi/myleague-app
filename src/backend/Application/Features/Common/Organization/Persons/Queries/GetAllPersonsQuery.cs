using Application.Common;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using MediatR;

public record GetAllPersonsQuery(
    int page = 1,
    int pageSize = 25,
    string? firstName = "",
    string? lastName = "",
    string? birthDate = "",
    bool? isRegistered = null
) : IRequest<Result<PagedResult<PersonDto>>>
{
    public const string ResourceKey = "persons";
}
