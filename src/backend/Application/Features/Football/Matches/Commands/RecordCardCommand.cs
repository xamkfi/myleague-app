using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record RecordCardCommand(
    Guid MatchId,
    Guid TeamId,
    Guid PlayerId,
    FootballCardType CardType,
    int PeriodNumber,
    int TimeInSeconds,
    string? Description) : IRequest<Result<FootballMatchDto>>;
