using Application.Common;
using Application.Features.Hockey.Officials.DTOs;
using Domain.Enums.Hockey.Teams;
using MediatR;

namespace Application.Features.Hockey.Officials.Commands;

/// <summary>
/// Creates a hockey official profile for a Common Person.
/// </summary>
public record CreateHockeyOfficialCommand(
    Guid PersonId,
    HockeyOfficialRole OfficialRole,
    string? OfficialNumber = null,
    DateTime? LicenseIssueDate = null,
    DateTime? LicenseExpiryDate = null) : IRequest<Result<HockeyOfficialDto>>;
