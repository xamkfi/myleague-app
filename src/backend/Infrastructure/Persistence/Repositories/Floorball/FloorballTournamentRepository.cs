using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball tournament repository
    /// </summary>
    public class FloorballTournamentRepository : RepositoryBase<FloorballTournament, FloorballDbContext>, IFloorballTournamentRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballTournamentRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballTournamentRepository(FloorballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball tournament by ID
        /// </summary>
        public async Task<FloorballTournament?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _entities
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        /// <summary>
        /// Gets a floorball tournament by ID with groups, group teams, and matches eagerly loaded.
        /// AsSplitQuery is used because we are loading multiple unrelated collections (Groups.Teams + Matches)
        /// and the cartesian product across them would otherwise inflate the result set.
        /// </summary>
        public async Task<FloorballTournament?> GetByIdWithGroupsAsync(Guid id, CancellationToken ct = default)
        {
            return await _entities
                .Include(t => t.Teams)
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .Include(t => t.Matches)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        /// <summary>
        /// Gets a floorball tournament by ID with groups eagerly loaded but without change tracking.
        /// Matches are intentionally not loaded — this overload is used by command handlers that only
        /// need to read group/team state and then persist a child entity directly via AddGroupAsync /
        /// AddGroupTeamAsync (avoids change-tracking the parent tournament aggregate).
        /// </summary>
        public async Task<FloorballTournament?> GetByIdWithGroupsAsNoTrackingAsync(Guid id, CancellationToken ct = default)
        {
            return await _entities
                .AsNoTracking()
                .Include(t => t.Teams)
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        /// <summary>
        /// Gets a tournament group by its ID with teams eagerly loaded.
        /// </summary>
        public async Task<FloorballTournamentGroup?> GetGroupByIdAsync(Guid groupId, CancellationToken ct = default)
        {
            return await _dbContext.Set<FloorballTournamentGroup>()
                .Include(g => g.Teams)
                    .ThenInclude(gt => gt.Team)
                .FirstOrDefaultAsync(g => g.Id == groupId, ct);
        }

        /// <summary>
        /// Adds a new tournament group directly to the persistence store without loading the parent
        /// tournament into the change tracker.
        /// </summary>
        public async Task AddGroupAsync(FloorballTournamentGroup group, CancellationToken ct = default)
        {
            await _dbContext.Set<FloorballTournamentGroup>().AddAsync(group, ct);
        }

        /// <summary>
        /// Adds a new tournament group/team join entity directly to the persistence store.
        /// </summary>
        public async Task AddGroupTeamAsync(FloorballTournamentGroupTeam groupTeam, CancellationToken ct = default)
        {
            await _dbContext.Set<FloorballTournamentGroupTeam>().AddAsync(groupTeam, ct);
        }

        /// <summary>
        /// Removes a tournament group/team join entity directly from the persistence store. The entity
        /// is attached so EF Core treats the row as deleted without requiring the parent aggregate to
        /// be tracked.
        /// </summary>
        public async Task RemoveGroupTeamAsync(FloorballTournamentGroupTeam groupTeam, CancellationToken ct = default)
        {
            DbSet<FloorballTournamentGroupTeam> set = _dbContext.Set<FloorballTournamentGroupTeam>();
            if (_dbContext.Entry(groupTeam).State == EntityState.Detached)
            {
                set.Attach(groupTeam);
            }
            set.Remove(groupTeam);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Inserts directly into the shared <c>FloorballCompetitionTeam</c> join table (mapped from the
        /// many-to-many <see cref="FloorballCompetition.Teams"/> relationship configured in
        /// <c>FloorballCompetitionConfiguration</c>). Going through the shadow Dictionary entity avoids
        /// loading and tracking the parent tournament aggregate, which keeps EF Core's TPH/owned-type
        /// change detection from issuing spurious updates on the parent row.
        /// </summary>
        public async Task AddCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default)
        {
            Dictionary<string, object> join = new()
            {
                ["CompetitionsId"] = competitionId,
                ["TeamsId"] = teamId
            };
            await _dbContext.Set<Dictionary<string, object>>("FloorballCompetitionTeam").AddAsync(join, ct);
        }

        /// <summary>
        /// Removes a row from the shared <c>FloorballCompetitionTeam</c> join table by composite key.
        /// Looks the row up by the (CompetitionsId, TeamsId) pair and deletes it through the shadow
        /// Dictionary entity so the parent tournament aggregate is not pulled into the change tracker.
        /// </summary>
        public async Task RemoveCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default)
        {
            Dictionary<string, object>? existing = await _dbContext
                .Set<Dictionary<string, object>>("FloorballCompetitionTeam")
                .FirstOrDefaultAsync(j =>
                    EF.Property<Guid>(j, "CompetitionsId") == competitionId &&
                    EF.Property<Guid>(j, "TeamsId") == teamId,
                    ct);

            if (existing != null)
            {
                _dbContext.Set<Dictionary<string, object>>("FloorballCompetitionTeam").Remove(existing);
            }
        }

        /// <summary>
        /// Checks whether a row exists in the <c>FloorballCompetitionTeam</c> join table for the given
        /// competition/team pair. Used by handlers to maintain idempotency when adding teams across
        /// multiple tournament groups (the parent join row should only be created once).
        /// </summary>
        public async Task<bool> ExistsCompetitionTeamAsync(Guid competitionId, Guid teamId, CancellationToken ct = default)
        {
            return await _dbContext
                .Set<Dictionary<string, object>>("FloorballCompetitionTeam")
                .AnyAsync(j =>
                    EF.Property<Guid>(j, "CompetitionsId") == competitionId &&
                    EF.Property<Guid>(j, "TeamsId") == teamId,
                    ct);
        }

        /// <summary>
        /// Gets all floorball tournaments. Eagerly loads Groups (with their teams) and Matches so that
        /// the listing DTO can report accurate teamCount/matchCount/group counts. AsSplitQuery is used to
        /// avoid the cartesian explosion that would otherwise occur when including multiple unrelated
        /// collections (Groups.Teams + Matches) on the same root.
        /// </summary>
        public async Task<List<FloorballTournament>> GetAllAsync(
            Domain.Enums.Common.TeamCategory? teamCategory = null,
            CancellationToken ct = default)
        {
            IQueryable<FloorballTournament> query = _entities
                .Include(t => t.Teams)
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .Include(t => t.Matches)
                .AsSplitQuery();

            if (teamCategory.HasValue)
            {
                query = query.Where(t => t.TeamCategory == teamCategory.Value);
            }

            return await query.ToListAsync(ct);
        }

        /// <summary>
        /// Gets active floorball tournaments. Same eager-loading strategy as GetAllAsync so the listing DTO
        /// can populate group/team/match counts.
        /// </summary>
        public async Task<List<FloorballTournament>> GetActiveAsync(
            Domain.Enums.Common.TeamCategory? teamCategory = null,
            CancellationToken ct = default)
        {
            IQueryable<FloorballTournament> query = _entities
                .Include(t => t.Teams)
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .Include(t => t.Matches)
                .AsSplitQuery()
                .Where(t => t.IsActive && t.TournamentStatus != FloorballTournamentStatus.Completed);

            if (teamCategory.HasValue)
            {
                query = query.Where(t => t.TeamCategory == teamCategory.Value);
            }

            return await query.ToListAsync(ct);
        }

        /// <summary>
        /// Adds a new floorball tournament
        /// </summary>
        public async Task AddAsync(FloorballTournament tournament, CancellationToken ct = default)
        {
            await _entities.AddAsync(tournament, ct);
        }

        /// <summary>
        /// Updates an existing floorball tournament
        /// </summary>
        public async Task UpdateAsync(FloorballTournament tournament, CancellationToken ct = default)
        {
            _dbContext.Entry(tournament).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a floorball tournament
        /// </summary>
        public async Task DeleteAsync(FloorballTournament tournament, CancellationToken ct = default)
        {
            _entities.Remove(tournament);
            await Task.CompletedTask;
        }
    }
}
