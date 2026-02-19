using System;
using Domain.Enums.Common;

namespace Application.Features.Common.Divisions.DTOs;

/// <summary>
/// Data Transfer Object for Division entity
/// </summary>
public record DivisionDto(
    Guid Id,
    string Name,
    string Description,
    int Level,
    SportsCategory SportType,
    bool IsActive,
    DateTime CreatedDate); 
