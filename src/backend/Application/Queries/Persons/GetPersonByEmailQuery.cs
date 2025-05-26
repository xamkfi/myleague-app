using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Queries.Persons;

/// <summary>
/// Query for retrieving a person by its email
/// </summary>
public record GetPersonByEmailQuery(string email) : IRequest<Result<PersonDto>>;
