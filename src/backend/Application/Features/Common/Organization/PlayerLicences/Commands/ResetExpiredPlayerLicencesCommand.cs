using Application.Common;
using Application.Features.Common.Organization.PlayerLicences.DTOs;
using MediatR;

namespace Application.Features.Common.Organization.PlayerLicences.Commands;

public record ResetExpiredPlayerLicencesCommand(bool Force = false)
    : IRequest<Result<PlayerLicenceResetResultDto>>;
