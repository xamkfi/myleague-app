using System;
using Application.Common;
using Application.DTOs.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for updating a person's registration status
/// </summary>
public record UpdatePersonRegistrationCommand(Guid Id, bool IsRegistered) : IRequest<Result<PersonDto>>; 