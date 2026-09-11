using Application.Common;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record DeleteFootballMatchCommand(Guid MatchId) : IRequest<Result>;
