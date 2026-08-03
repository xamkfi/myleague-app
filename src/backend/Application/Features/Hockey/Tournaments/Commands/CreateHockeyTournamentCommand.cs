using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

public record CreateHockeyTournamentCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Venue = null,
    string? ContentHtml = null) : IRequest<Result<HockeyTournamentDto>>;
