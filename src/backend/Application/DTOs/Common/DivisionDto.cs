using System;

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for Division entity
/// </summary>
public record DivisionDto(
    Guid Id,
    string Name,
    string Description,
    int Level,
    string SportType,
    bool IsActive,
    DateTime CreatedDate); 