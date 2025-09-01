using Application.Common;
using MediatR;

namespace Application.Commands.Floorball.Statistics;

/// <summary>
/// Command for updating statistics after a match is completed
/// </summary>
/// <param name="MatchId">The match ID</param>
public record UpdateMatchStatisticsCommand(Guid MatchId) : IRequest<Result>;
