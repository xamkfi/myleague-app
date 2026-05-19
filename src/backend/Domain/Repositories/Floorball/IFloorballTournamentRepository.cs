using Domain.Entities.Floorball;

namespace Domain.Repositories.Floorball;

/// <summary>
/// Repository for managing floorball tournaments
/// </summary>
public interface IFloorballTournamentRepository
{
    /// <summary>
    /// Gets a floorball tournament by ID
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The tournament if found, null otherwise</returns>
    Task<FloorballTournament?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a floorball tournament by ID with groups and group teams eagerly loaded
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The tournament with groups if found, null otherwise</returns>
    Task<FloorballTournament?> GetByIdWithGroupsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a floorball tournament by ID with groups eagerly loaded but without change tracking.
    /// Use this when you only need to read the tournament state (e.g. for validation or to compute
    /// derived values) and intend to make modifications via dedicated child-entity operations such
    /// as <see cref="AddGroupAsync"/>. Avoiding tracking sidesteps EF Core 9 TPH/owned-type change
    /// detection issues that can spuriously mark the parent as Modified.
    /// </summary>
    /// <param name="id">The tournament ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The tournament with groups if found, null otherwise</returns>
    Task<FloorballTournament?> GetByIdWithGroupsAsNoTrackingAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a tournament group by its ID with teams (and clubs) eagerly loaded.
    /// </summary>
    /// <param name="groupId">The tournament group ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The group with its teams if found, null otherwise</returns>
    Task<FloorballTournamentGroup?> GetGroupByIdAsync(Guid groupId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new tournament group directly to the persistence store. Avoids loading the parent
    /// tournament into the change tracker, which prevents EF Core's TPH/owned-type change detection
    /// from issuing spurious UPDATE statements against the parent <see cref="FloorballTournament"/> row.
    /// </summary>
    /// <param name="group">The tournament group to add</param>
    /// <param name="ct">Cancellation token</param>
    Task AddGroupAsync(FloorballTournamentGroup group, CancellationToken ct = default);

    /// <summary>
    /// Adds a new tournament group/team join entity directly to the persistence store, side-stepping
    /// any change tracking on the parent tournament/group entities (see <see cref="AddGroupAsync"/>
    /// for the rationale).
    /// </summary>
    /// <param name="groupTeam">The tournament group/team join to add</param>
    /// <param name="ct">Cancellation token</param>
    Task AddGroupTeamAsync(FloorballTournamentGroupTeam groupTeam, CancellationToken ct = default);

    /// <summary>
    /// Removes a tournament group/team join entity directly from the persistence store, without
    /// loading the parent aggregate into the change tracker.
    /// </summary>
    /// <param name="groupTeam">The tournament group/team join to remove</param>
    /// <param name="ct">Cancellation token</param>
    Task RemoveGroupTeamAsync(FloorballTournamentGroupTeam groupTeam, CancellationToken ct = default);

    /// <summary>
    /// Inserts a row into the parent <c>FloorballCompetitionTeam</c> join table directly. Used to keep
    /// the inherited <see cref="FloorballCompetition.Teams"/> collection in sync whenever a team is
    /// added to a tournament group, without loading the tracked parent aggregate. Caller is responsible
    /// for ensuring the join doesn't already exist (use <see cref="ExistsCompetitionTeamAsync"/> to
    /// guard against duplicates).
    /// </summary>
    /// <param name="competitionId">The competition (tournament) id</param>
    /// <param name="teamId">The team id</param>
    /// <param name="ct">Cancellation token</param>
    Task AddCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default);

    /// <summary>
    /// Removes a row from the parent <c>FloorballCompetitionTeam</c> join table directly. No-op when
    /// no matching row exists. Used to keep the parent collection consistent when a team leaves every
    /// group it belonged to in a tournament.
    /// </summary>
    /// <param name="competitionId">The competition (tournament) id</param>
    /// <param name="teamId">The team id</param>
    /// <param name="ct">Cancellation token</param>
    Task RemoveCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default);

    /// <summary>
    /// Returns true when the parent <c>FloorballCompetitionTeam</c> join table contains a row for the
    /// given competition + team pair.
    /// </summary>
    Task<bool> ExistsCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default);

    /// <summary>
    /// Gets all floorball tournaments
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A list of all floorball tournaments</returns>
    Task<List<FloorballTournament>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets active floorball tournaments (not completed)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A list of active floorball tournaments</returns>
    Task<List<FloorballTournament>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to add</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(FloorballTournament tournament, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to update</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateAsync(FloorballTournament tournament, CancellationToken ct = default);

    /// <summary>
    /// Deletes a floorball tournament
    /// </summary>
    /// <param name="tournament">The tournament to delete</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteAsync(FloorballTournament tournament, CancellationToken ct = default);
}
