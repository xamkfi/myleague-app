using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Queries;

public record GetTodaysMatchesByTeamQuery(Guid TeamId) : IRequest<Result<IEnumerable<FootballMatchDto>>>;
