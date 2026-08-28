using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record StartFootballMatchCommand(Guid Id) : IRequest<Result<FootballMatchDto>>;
