using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record AddTeamToSeasonCommand(
    Guid CompetitionId,
    Guid TeamId,
    Domain.Enums.Common.RosterEnrollmentMode RosterMode = Domain.Enums.Common.RosterEnrollmentMode.CopyLatest) : IRequest<Result<FootballSeasonDto>>;
