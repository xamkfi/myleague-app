using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Persons;
/// <summary>
/// Query for retrieving a person by its ID
/// </summary>
public record GetPersonByIdQuery(Guid PersonId) : IRequest<Result<PersonDto>>;
    
    

