using System;
using Application.Common;
using Application.DTOs.Common;
using Domain.ValueObjects.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for updating an existing ContactInfo
/// </summary>
/// <param name="Id"></param>
/// <param name="contactInfo"></param>
public record UpdatePersonContactInfoCommand(Guid Id, ContactInfo contactInfo) : IRequest<Result<ContactInfoDto>>;
