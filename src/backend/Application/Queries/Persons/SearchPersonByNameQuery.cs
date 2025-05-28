using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Persons;

/// <summary>
/// Query for retrieving a person by its firstName or lastName
/// </summary>
public record SearchPersonByNameQuery(string name) : IRequest<Result<IEnumerable<PersonDto>>>;
    
   

