using System;
using Application.Common;
using Application.DTOs.Common;
using Domain.Common;
using MediatR;

namespace Application.Queries.Persons;

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

