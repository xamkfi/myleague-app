using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Shared.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.DataSubjectRights.Commands;

/// <summary>
/// Corrects identity data the person supplied.
/// </summary>
public record RectifyDataSubjectCommand(
    Guid PersonId,
    string FirstName,
    string LastName,
    DateTime? BirthDate,
    AddressDto? Address,
    ContactInfoDto? ContactInfo,
    string? AccountEmail) : IRequest<Result<DataSubjectCopyDto>>;
