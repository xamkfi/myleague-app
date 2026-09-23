using Application.Common;
using MediatR;

namespace Application.Features.Football.Competitions.Commands;

public record AddFootballTeamToSeasonDivisionCommand(
    Guid CompetitionId,
    Guid DivisionId,
    Guid TeamId,
    Domain.Enums.Common.RosterEnrollmentMode RosterMode = Domain.Enums.Common.RosterEnrollmentMode.CopyLatest) : IRequest<Result>;
