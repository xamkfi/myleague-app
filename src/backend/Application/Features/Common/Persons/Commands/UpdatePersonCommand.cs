using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for updating an existing Person
/// </summary>
public record UpdatePersonCommand(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime? BirthDate,
    bool IsRegistered,
    AddressDto? Address,
    ContactInfoDto? ContactInfo) : IRequest<Result<PersonDto>>;


