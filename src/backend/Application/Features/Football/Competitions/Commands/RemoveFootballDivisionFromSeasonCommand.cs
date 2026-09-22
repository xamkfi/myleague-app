using Application.Common;
using MediatR;

namespace Application.Features.Football.Competitions.Commands;

public record RemoveFootballDivisionFromSeasonCommand(Guid CompetitionId, Guid DivisionId) : IRequest<Result>;
