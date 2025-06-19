using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Persons;

/// <summary>
/// Query for retrieving a person with their teams
/// </summary>
public record GetPersonWithTeamsQuery(Guid PersonId) : IRequest<Result<PersonWithTeamsDto>>; 