using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for updating person basic info
/// </summary>
public record UpdatePersonBasicInfoCommand(Guid Id, string FirstName, string LastName) : IRequest<Result<PersonDto>>;

