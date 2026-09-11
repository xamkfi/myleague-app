using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record DeactivateFootballSeasonCommand(Guid Id) : IRequest<Result<FootballSeasonDto>>;
