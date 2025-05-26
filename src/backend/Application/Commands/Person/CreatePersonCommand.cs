using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Commands.Person;

/// <summary>
/// Command for creating a new person
/// </summary>
public record CreatePersonCommand(
    string FirstName,
    string LastName,
    DateTime BirthDate,
    AddressDto? Address,
    ContactInfoDto? ContactInfo) : IRequest<Result<PersonDto>>;


