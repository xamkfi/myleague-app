using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for creating a new person
/// </summary>
public record CreatePersonCommand(
    string FirstName,
    string LastName,
    DateTime? BirthDate = null,
    bool IsRegistered = false,
    AddressDto? Address = null,
    ContactInfoDto? ContactInfo = null) : IRequest<Result<PersonDto>>;


