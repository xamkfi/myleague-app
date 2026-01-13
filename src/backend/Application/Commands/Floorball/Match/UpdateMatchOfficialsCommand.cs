using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match;

/// <summary>
/// Command to replace the officials for a match.
/// </summary>
/// <param name="MatchId">Match identifier</param>
/// <param name="OfficialIds">Officials to keep (must be at least one)</param>
public record UpdateMatchOfficialsCommand(Guid MatchId, IReadOnlyCollection<Guid> OfficialIds) : IRequest<Result<FloorballMatchDto>>;

