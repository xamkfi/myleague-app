using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record UpdateMatchOfficialsCommand(Guid MatchId, IReadOnlyCollection<Guid> OfficialIds) : IRequest<Result<FootballMatchDto>>;
