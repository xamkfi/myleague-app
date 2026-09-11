using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record DeleteSubstitutionCommand(Guid MatchId, Guid SubstitutionEventId) : IRequest<Result<FootballMatchDto>>;
