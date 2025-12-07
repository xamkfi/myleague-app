using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;
using Domain.Enums.Common;

namespace Application.Commands.Common;

/// <summary>
/// Command for creating a new division
/// </summary>
public record CreateDivisionCommand(
    string Name,
    string Description,
    int Level,
    SportsCategory SportType) : IRequest<Result<DivisionDto>>;