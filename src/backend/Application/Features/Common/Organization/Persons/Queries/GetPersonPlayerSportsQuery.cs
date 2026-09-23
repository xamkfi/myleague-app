using Application.Common;
using Application.Features.Common.Organization.Persons.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.Persons.Queries;

/// <summary>
/// Resolves a person (or a sport-specific player id) and returns every sport
/// player profile linked to that person.
/// </summary>
public record GetPersonPlayerSportsQuery(Guid Id) : IRequest<Result<PersonPlayerSportsDto>>;
