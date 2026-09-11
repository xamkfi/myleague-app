using Application.Common;
using Application.Features.Hockey.Players.DTOs;
using MediatR;

namespace Application.Features.Hockey.Players.Queries;

/// <summary>
/// Query to retrieve a hockey player by id.
/// </summary>
public record GetHockeyPlayerByIdQuery(Guid Id) : IRequest<Result<HockeyPlayerDto>>;
