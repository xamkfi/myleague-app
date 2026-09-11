using Application.Common;
using Application.Features.Football.Seasons.DTOs;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record RemoveTeamFromSeasonCommand(Guid CompetitionId, Guid TeamId) : IRequest<Result<FootballSeasonDto>>;
