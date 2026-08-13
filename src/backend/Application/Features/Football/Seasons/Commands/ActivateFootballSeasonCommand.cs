using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record ActivateFootballSeasonCommand(Guid Id) : IRequest<Result<FootballSeasonDto>>;
