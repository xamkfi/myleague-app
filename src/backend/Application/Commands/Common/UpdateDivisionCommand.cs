using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Commands.Common;

/// <summary>
/// Command for updating an existing division
/// </summary>
public record UpdateDivisionCommand(
    Guid Id,
    string Name,
    string Description,
    int Level) : IRequest<Result<DivisionDto>>; 