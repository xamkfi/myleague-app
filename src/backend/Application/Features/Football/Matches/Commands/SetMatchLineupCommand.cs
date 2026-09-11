using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

/// <summary>
/// Replaces a team's match squad and on-field lineup. Goalkeeper is a position in the lineup.
/// </summary>
public class SetMatchLineupCommand : IRequest<Result<FootballMatchDto>>
{
    public Guid MatchId { get; set; }
    public Guid TeamId { get; set; }
    public List<LineupPlayerInput> Players { get; set; } = new();
}

public class LineupPlayerInput
{
    public Guid PlayerId { get; set; }
    public FootballPosition Position { get; set; }
    public bool IsOnField { get; set; }
}
