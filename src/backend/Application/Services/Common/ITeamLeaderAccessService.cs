namespace Application.Services.Common;

/// <summary>
/// Service for checking whether a person (team leader) is allowed to manage a specific team.
/// A person can manage a team when an active team manager row links them to that team.
/// </summary>
public interface ITeamLeaderAccessService
{
    /// <summary>
    /// Checks whether the person is an active manager of the given floorball team
    /// </summary>
    /// <param name="personId">The person ID from the caller's token</param>
    /// <param name="teamId">The floorball team ID</param>
    /// <returns>True if the person may manage the team</returns>
    Task<bool> CanManageFloorballTeamAsync(Guid personId, Guid teamId);

    /// <summary>
    /// Checks whether the person is an active manager of the given football team
    /// </summary>
    /// <param name="personId">The person ID from the caller's token</param>
    /// <param name="teamId">The football team ID</param>
    /// <returns>True if the person may manage the team</returns>
    Task<bool> CanManageFootballTeamAsync(Guid personId, Guid teamId);
}
