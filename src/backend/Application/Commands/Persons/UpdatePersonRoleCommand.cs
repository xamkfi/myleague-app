using System;
using Application.Common;
using Application.DTOs.Common;
using Domain.Enums.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for updating person role
/// </summary>
public record UpdatePersonRoleCommand(
    Guid Id,
    PersonRole Role) : IRequest<Result<PersonDto>>; 