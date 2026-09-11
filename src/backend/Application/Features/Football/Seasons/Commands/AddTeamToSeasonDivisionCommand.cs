using Application.Common;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record AddTeamToSeasonDivisionCommand(Guid CompetitionId, Guid DivisionId, Guid TeamId) : IRequest<Result>;
