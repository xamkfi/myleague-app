using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Queries;

public record GetFootballMatchesBySeasonQuery(Guid CompetitionId) : IRequest<Result<IEnumerable<FootballMatchDto>>>;
