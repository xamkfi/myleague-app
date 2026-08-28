using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Queries;

public record GetFootballSeasonByIdQuery(Guid Id) : IRequest<Result<FootballSeasonDto>>;
