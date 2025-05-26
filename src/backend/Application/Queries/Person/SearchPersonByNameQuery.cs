using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Person;

/// <summary>
/// Query for retrieving a person by its firstName or lastName
/// </summary>
public record SearchPersonByNameQuery(string name) : IRequest<Result<PersonDto>>;
    
   

