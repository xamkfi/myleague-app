using System;
using MediatR;
using Application.Common;

namespace Application.Features.Common.Divisions.Commands;

/// <summary>
/// Command for activating a division
/// </summary>
public record ActivateDivisionCommand(Guid Id) : IRequest<Result<bool>>; 
