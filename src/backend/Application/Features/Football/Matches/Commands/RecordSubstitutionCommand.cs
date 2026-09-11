using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record RecordSubstitutionCommand(
    Guid MatchId,
    Guid TeamId,
    Guid PlayerOffId,
    Guid PlayerOnId,
    int PeriodNumber,
    int TimeInSeconds,
    string? Description) : IRequest<Result<FootballMatchDto>>;
