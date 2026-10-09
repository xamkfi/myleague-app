using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

/// <summary>
/// Puts a match that was started by mistake back to Scheduled. Allowed only while the match is
/// in progress with a 0-0 score and no recorded events.
/// </summary>
/// <param name="Id">Match identifier.</param>
public record RevertFootballMatchToScheduledCommand(Guid Id) : IRequest<Result<FootballMatchDto>>;
