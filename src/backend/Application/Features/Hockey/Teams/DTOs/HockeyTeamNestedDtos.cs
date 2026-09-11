namespace Application.Features.Hockey.Teams.DTOs;

/// <summary>
/// Data transfer object for a hockey team roster membership.
/// </summary>
public record HockeyTeamPlayerDto(
    Guid Id,
    Guid TeamId,
    Guid PlayerId,
    Guid? CompetitionId,
    string Position,
    string CaptainRole,
    string RosterStatus,
    int? JerseyNumber,
    int? RequestedJerseyNumber,
    bool IsActive,
    DateTime JoinedAt);

/// <summary>
/// Data transfer object for a hockey line.
/// </summary>
public record HockeyLineDto(
    Guid Id,
    Guid TeamId,
    Guid? CompetitionId,
    string Name,
    int LineNumber,
    string LineType,
    bool IsActive,
    IReadOnlyCollection<HockeyLinePlayerDto> Players);

/// <summary>
/// Data transfer object for a player assignment on a hockey line.
/// </summary>
public record HockeyLinePlayerDto(
    Guid Id,
    Guid LineId,
    Guid TeamPlayerId,
    string Slot,
    int Order);

/// <summary>
/// Data transfer object for a hockey team staff member.
/// </summary>
public record HockeyTeamStaffDto(
    Guid Id,
    Guid TeamId,
    Guid PersonId,
    Guid? CompetitionId,
    string Role,
    bool IsActive,
    DateTime JoinedAt);
