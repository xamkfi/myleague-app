using Application.Common;
using Application.Features.Common.PlayerLicences.DTOs;
using MediatR;

namespace Application.Features.Common.PlayerLicences.Commands;

public record ResetExpiredPlayerLicencesCommand(bool Force = false)
    : IRequest<Result<PlayerLicenceResetResultDto>>;
