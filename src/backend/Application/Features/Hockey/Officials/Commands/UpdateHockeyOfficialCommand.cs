using Application.Common;
using Application.Features.Hockey.Officials.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Officials.Commands;

/// <summary>
/// Updates an existing hockey official profile.
/// </summary>
public record UpdateHockeyOfficialCommand(
    Guid OfficialId,
    HockeyOfficialRole OfficialRole,
    string? OfficialNumber,
    DateTime? LicenseIssueDate,
    DateTime? LicenseExpiryDate,
    bool IsActive) : IRequest<Result<HockeyOfficialDto>>;
