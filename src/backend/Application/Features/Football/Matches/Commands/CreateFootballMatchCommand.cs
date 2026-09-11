using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

public record CreateFootballMatchCommand(
    Guid? CompetitionId,
    Guid? HomeTeamId,
    Guid? AwayTeamId,
    Guid? RefereeId,
    DateTime ScheduledDateTime,
    string? Venue,
    Guid? TournamentGroupId = null,
    FootballTournamentStage? TournamentStage = null) : IRequest<Result<FootballMatchDto>>;
