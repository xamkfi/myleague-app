using Application.Common;
using MediatR;

namespace Application.Features.Football.Competitions.Commands;

public record RemoveFootballTeamFromSeasonDivisionCommand(Guid CompetitionId, Guid DivisionId, Guid TeamId) : IRequest<Result>;
