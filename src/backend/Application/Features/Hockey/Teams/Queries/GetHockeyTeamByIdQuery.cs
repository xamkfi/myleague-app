using Application.Common;
using Application.Features.Hockey.Teams.DTOs;
using MediatR;

namespace Application.Features.Hockey.Teams.Queries;

public record GetHockeyTeamByIdQuery(Guid Id) : IRequest<Result<HockeyTeamDto>>;
