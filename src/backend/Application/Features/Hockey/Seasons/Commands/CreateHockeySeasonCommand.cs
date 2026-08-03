using Application.Common;
using Application.Features.Hockey.Seasons.DTOs;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

public record CreateHockeySeasonCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? SeasonCode = null) : IRequest<Result<HockeySeasonDto>>;
