using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.DataSubjectRights.Commands;

/// <summary>
/// Restricts or lifts restriction of processing for one person.
/// </summary>
public record SetDataSubjectRestrictionCommand(Guid PersonId, bool IsRestricted) : IRequest<Result<DataSubjectCopyDto>>;
