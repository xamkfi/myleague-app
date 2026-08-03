using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Queries;

public record GetHockeyTournamentByIdQuery(Guid Id) : IRequest<Result<HockeyTournamentDto>>;
