using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record UpdateFootballMatchOfficialsCommand(Guid MatchId, IReadOnlyCollection<Guid> OfficialIds) : IRequest<Result<FootballMatchDto>>;
