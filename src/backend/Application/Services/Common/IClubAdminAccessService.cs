namespace Application.Services.Common;

/// <summary>
/// Service for checking whether a person (club admin) is allowed to manage a specific club
/// or the teams under it. A person can manage a club when an active club manager row links
/// them to that club; team access is derived from the team's club.
/// </summary>
public interface IClubAdminAccessService
{
    /// <summary>
    /// Checks whether the person is an active manager of the given club
    /// </summary>
    /// <param name="personId">The person ID from the caller's token</param>
    /// <param name="clubId">The club ID</param>
    /// <returns>True if the person may manage the club</returns>
    Task<bool> CanManageClubAsync(Guid personId, Guid clubId);

    /// <summary>
    /// Checks whether the person manages the club that owns the given floorball team
    /// </summary>
    /// <param name="personId">The person ID from the caller's token</param>
    /// <param name="teamId">The floorball team ID</param>
    /// <returns>True if the person may manage the team</returns>
    Task<bool> CanManageFloorballTeamAsync(Guid personId, Guid teamId);

    /// <summary>
    /// Checks whether the person manages the club that owns the given football team
    /// </summary>
    /// <param name="personId">The person ID from the caller's token</param>
    /// <param name="teamId">The football team ID</param>
    /// <returns>True if the person may manage the team</returns>
    Task<bool> CanManageFootballTeamAsync(Guid personId, Guid teamId);
}
