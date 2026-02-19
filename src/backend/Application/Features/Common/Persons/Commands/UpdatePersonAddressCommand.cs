using System;
using Application.Common;
using Application.DTOs.Common;
using Domain.ValueObjects.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for updating an existing Address
/// </summary>
public record UpdatePersonAddressCommand(Guid Id, Address address) : IRequest<Result<AddressDto>>;

