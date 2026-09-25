using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.DataSubjectRights.Commands;

/// <summary>
/// Removes identity data where processing no longer has a basis. League rows stay under an anonymized name.
/// </summary>
public record EraseDataSubjectCommand(Guid PersonId) : IRequest<Result<DataSubjectCopyDto>>;
