using Application.Common;
using MediatR;

namespace Application.Features.Football.Seasons.Commands;

public record DeleteFootballSeasonCommand(Guid Id) : IRequest<Result>;
