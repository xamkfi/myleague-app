using Application.Common;
using MediatR;

namespace Application.Features.Hockey.Seasons.Commands;

/// <summary>
/// Deletes a hockey season when every match is still unplayed.
/// </summary>
public record DeleteHockeySeasonCommand(Guid Id) : IRequest<Result>;
