using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.DataSubjectRights.Queries;

/// <summary>
/// Returns whether a person's data is processed and a copy of that data.
/// </summary>
public record GetDataSubjectCopyQuery(Guid PersonId) : IRequest<Result<DataSubjectCopyDto>>;
