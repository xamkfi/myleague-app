using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Queries;

public record GetHockeySeasonByIdQuery(Guid Id) : IRequest<Result<HockeySeasonDto>>;
