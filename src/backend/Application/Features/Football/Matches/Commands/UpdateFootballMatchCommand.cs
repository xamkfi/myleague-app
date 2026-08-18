using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record UpdateFootballMatchCommand(
    Guid Id,
    DateTime ScheduledDateTime,
    string? Venue) : IRequest<Result<FootballMatchDto>>;
