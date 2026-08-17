namespace Application.Features.Football.Tournaments.DTOs;

/// <summary>
/// Aggregate DTO returned by GET /api/footballtournament/{id}/playoff-bracket.
/// Contains the full playoff bracket grouped by round, plus the champion (if the final has been
/// completed) so the frontend can render its callout in a single round-trip.
/// </summary>
/// <param name="TournamentId">The tournament id this bracket belongs to.</param>
/// <param name="TournamentStatus">String form of the current tournament lifecycle status (Draft, GroupStage, PlayoffStage, Completed, Cancelled).</param>
/// <param name="HasThirdPlaceMatch">Whether the bracket includes a 3rd-place match.</param>
/// <param name="Champion">Set once the final has been completed.</param>
/// <param name="Rounds">The rounds in display order (Quarterfinals -> Semifinals -> [3rd place] -> Final).</param>
public record FootballPlayoffBracketDto(
    Guid TournamentId,
    string TournamentStatus,
    bool HasThirdPlaceMatch,
    FootballPlayoffTeamDto? Champion,
    List<FootballPlayoffRoundDto> Rounds);

/// <summary>
/// One round of the bracket (Quarterfinal, Semifinal, ThirdPlaceMatch, Final).
/// </summary>
public record FootballPlayoffRoundDto(
    string Round,
    List<FootballPlayoffMatchDto> Matches);

/// <summary>
/// Single playoff match card. Scores are 0/0 until the match is in progress.
/// HomeTeam/AwayTeam may still be present even when feeder matches are unfinished — the frontend
/// renders TBD by checking <see cref="IsFeederResolved"/> instead of relying on team being null.
/// </summary>
public record FootballPlayoffMatchDto(
    Guid MatchId,
    int Order,
    string Status,
    DateTime ScheduledDateTime,
    string? Venue,
    int HomeScore,
    int AwayScore,
    FootballPlayoffTeamDto? HomeTeam,
    FootballPlayoffTeamDto? AwayTeam,
    bool IsHomeFeederResolved,
    bool IsAwayFeederResolved,
    Guid? NextMatchId,
    string? NextMatchSlot);

/// <summary>
/// Compact team representation used inside the bracket DTO.
/// </summary>
public record FootballPlayoffTeamDto(
    Guid TeamId,
    string TeamName,
    string? TeamLogo);
