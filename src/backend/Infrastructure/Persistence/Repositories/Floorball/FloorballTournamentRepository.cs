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
        /// Gets a floorball tournament by ID with groups and group teams eagerly loaded
        /// </summary>
        public async Task<FloorballTournament?> GetByIdWithGroupsAsync(Guid id, CancellationToken ct = default)
        {
            return await _entities
                .Include(t => t.Teams)
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        /// <summary>
        /// Gets all floorball tournaments
        /// </summary>
        public async Task<List<FloorballTournament>> GetAllAsync(CancellationToken ct = default)
        {
            return await _entities
                .Include(t => t.Teams)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Gets active floorball tournaments
        /// </summary>
        public async Task<List<FloorballTournament>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _entities
                .Include(t => t.Teams)
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
