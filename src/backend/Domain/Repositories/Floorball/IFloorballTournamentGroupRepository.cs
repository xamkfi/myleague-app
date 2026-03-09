using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball.Tournament;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball tournament groups and their team memberships
/// </summary>
public interface IFloorballTournamentGroupRepository
{
    /// <summary>
    /// Gets a tournament group by ID, including team memberships
    /// </summary>
    Task<FloorballTournamentGroup?> GetByIdAsync(Guid groupId);

    /// <summary>
    /// Gets all groups for a given tournament, including team memberships
    /// </summary>
    Task<IEnumerable<FloorballTournamentGroup>> GetByTournamentIdAsync(Guid tournamentId);

    /// <summary>
    /// Adds a new group to a tournament
    /// </summary>
    Task AddAsync(FloorballTournamentGroup group);

    /// <summary>
    /// Updates an existing group
    /// </summary>
    Task UpdateAsync(FloorballTournamentGroup group);

    /// <summary>
    /// Deletes a group by ID
    /// </summary>
    Task DeleteAsync(Guid groupId);

    /// <summary>
    /// Checks if a group exists
    /// </summary>
    Task<bool> ExistsAsync(Guid groupId);

    /// <summary>
    /// Adds a team to a tournament group
    /// </summary>
    Task<FloorballTournamentGroupTeam> AddTeamToGroupAsync(Guid groupId, Guid teamId, Guid tournamentId);

    /// <summary>
    /// Removes a team from a tournament group
    /// </summary>
    Task RemoveTeamFromGroupAsync(Guid groupId, Guid teamId);

    /// <summary>
    /// Gets all team memberships in a given group, including team navigation
    /// </summary>
    Task<IEnumerable<FloorballTournamentGroupTeam>> GetTeamsByGroupIdAsync(Guid groupId);

    /// <summary>
    /// Gets all groups that a specific team belongs to within a tournament
    /// </summary>
    Task<IEnumerable<FloorballTournamentGroup>> GetGroupsByTeamIdAsync(Guid tournamentId, Guid teamId);
}
