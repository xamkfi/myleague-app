using Application.Common;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record RemoveDivisionFromSeasonCommand(Guid CompetitionId, Guid DivisionId) : IRequest<Result>;
