using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Persons;
/// <summary>
/// Query for retrieving all persons
/// </summary>
public record GetAllPersonsQuery() : IRequest<Result<IEnumerable<PersonDto>>>;
