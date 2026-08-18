using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record RemoveOfficialFromMatchCommand(Guid MatchId, Guid RefereeId) : IRequest<Result<FootballMatchDto>>;
