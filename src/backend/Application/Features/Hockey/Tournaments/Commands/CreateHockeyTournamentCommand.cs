using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command for creating a hockey tournament.
/// </summary>
/// <param name="Name">Name of the tournament</param>
/// <param name="StartDate">Tournament start date</param>
/// <param name="EndDate">Tournament end date</param>
/// <param name="Venue">Optional primary venue</param>
/// <param name="ContentHtml">Optional HTML description</param>
public record CreateHockeyTournamentCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Venue = null,
    string? ContentHtml = null) : IRequest<Result<HockeyTournamentDto>>;
