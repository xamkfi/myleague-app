using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record DeleteCardCommand(Guid MatchId, Guid CardEventId) : IRequest<Result<FootballMatchDto>>;
