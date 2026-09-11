using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Queries;

/// <summary>
/// Query for retrieving a hockey tournament by id.
/// </summary>
/// <param name="Id">Tournament id</param>
public record GetHockeyTournamentByIdQuery(Guid Id) : IRequest<Result<HockeyTournamentDto>>;
