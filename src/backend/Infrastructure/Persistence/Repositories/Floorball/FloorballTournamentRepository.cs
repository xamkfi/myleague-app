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
        /// Gets all floorball tournaments. Eagerly loads Groups (with their teams) and Matches so that
        /// the listing DTO can report accurate teamCount/matchCount/group counts. AsSplitQuery is used to
        /// avoid the cartesian explosion that would otherwise occur when including multiple unrelated
        /// collections (Groups.Teams + Matches) on the same root.
        /// </summary>
        public async Task<List<FloorballTournament>> GetAllAsync(CancellationToken ct = default)
        {
            return await _entities
                .Include(t => t.Teams)
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .Include(t => t.Matches)
                .AsSplitQuery()
                .ToListAsync(ct);
        }

        /// <summary>
        /// Gets active floorball tournaments. Same eager-loading strategy as GetAllAsync so the listing DTO
        /// can populate group/team/match counts.
        /// </summary>
        public async Task<List<FloorballTournament>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _entities
                .Include(t => t.Teams)
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .Include(t => t.Matches)
                .AsSplitQuery()
                .Where(t => t.IsActive && t.TournamentStatus != FloorballTournamentStatus.Completed)
                .ToListAsync(ct);
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
