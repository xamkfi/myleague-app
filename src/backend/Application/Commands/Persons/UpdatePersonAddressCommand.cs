// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Common;
using Domain.ValueObjects.Common;
using MediatR;

namespace Application.Commands.Persons;

/// <summary>
/// Command for updating an existing Address
/// </summary>
public record UpdatePersonAddressCommand(Guid Id, Address address) : IRequest<Result<AddressDto>>;

