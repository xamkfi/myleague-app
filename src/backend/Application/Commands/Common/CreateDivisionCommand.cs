using System;
using MediatR;
using Application.DTOs.Common;
using Application.Common;

namespace Application.Commands.Common;

/// <summary>
/// Command for creating a new division
/// </summary>
public record CreateDivisionCommand(
    string Name,
    string Description,
    int Level,
    string SportType) : IRequest<Result<DivisionDto>>; 