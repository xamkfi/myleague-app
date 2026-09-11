using System;
using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Common.Persons.Queries;

/// <summary>
/// Query for retrieving a person by its firstName or lastName
/// </summary>
public record SearchPersonByNameQuery(
    string name,
    int page = 1,
    int pageSize = 25
) : IRequest<Result<PagedResult<PersonDto>>>
{
    public const string ResourceKey = "persons";
}

