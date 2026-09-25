using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.DataSubjectRights.Commands;

/// <summary>
/// Records or withdraws an objection to legitimate-interest processing.
/// </summary>
public record SetDataSubjectObjectionCommand(Guid PersonId, bool HasObjected) : IRequest<Result<DataSubjectCopyDto>>;
