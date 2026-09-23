using System;
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

namespace Application.Features.Common.Organization.Persons.Queries;

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

