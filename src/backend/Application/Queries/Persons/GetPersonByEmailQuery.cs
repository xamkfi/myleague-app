using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Persons;

/// <summary>
/// Query for retrieving a person by its email
/// </summary>
public record GetPersonByEmailQuery(string email) : IRequest<Result<PersonDto>>;
