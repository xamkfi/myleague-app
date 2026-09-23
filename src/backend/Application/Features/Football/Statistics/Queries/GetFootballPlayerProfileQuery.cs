using Application.Common;
using Application.Features.Football.Players.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving a player profile with career statistics
/// </summary>
public record GetFootballPlayerProfileQuery(Guid playerId) : IRequest<Result<FootballPlayerProfileDto>>;
