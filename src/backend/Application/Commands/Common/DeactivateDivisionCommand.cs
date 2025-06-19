using System;
using MediatR;
using Application.Common;

namespace Application.Commands.Common;

/// <summary>
/// Command for deactivating a division
/// </summary>
public record DeactivateDivisionCommand(Guid Id) : IRequest<Result<bool>>; 