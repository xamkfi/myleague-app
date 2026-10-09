using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Puts a match that was started by mistake back to Scheduled. Allowed only while the match is
/// live with a 0-0 score and no events other than period markers.
/// </summary>
public record RevertHockeyMatchToScheduledCommand(Guid MatchId) : IRequest<Result<HockeyMatchDto>>;
