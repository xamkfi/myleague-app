using Application.Common;
using MediatR;

namespace Application.Features.Hockey.Players.Commands;

/// <summary>
/// Deletes a hockey player profile after leaving team rosters.
/// </summary>
public record DeleteHockeyPlayerCommand(Guid Id) : IRequest<Result>;
