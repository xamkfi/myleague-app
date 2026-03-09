using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball.Tournament;
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
        /// Gets a floorball tournament by ID with all related data
        /// </summary>
        /// <param name="id">The tournament ID</param>
        /// <returns>The tournament if found, null otherwise</returns>
        public override async Task<FloorballTournament?> GetByIdAsync(Guid id)
        {
            return await _entities
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.HomeTeam)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.AwayTeam)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Gets a floorball tournament by ID with groups and their teams
        /// </summary>
        /// <param name="id">The tournament ID</param>
        /// <returns>The tournament if found, null otherwise</returns>
        public async Task<FloorballTournament?> GetByIdWithGroupsAsync(Guid id)
        {
            return await _entities
                .Include(t => t.Groups)
                    .ThenInclude(g => g.Teams)
                        .ThenInclude(gt => gt.Team)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Gets all floorball tournaments
        /// </summary>
        /// <returns>A collection of all floorball tournaments</returns>
        public override async Task<IEnumerable<FloorballTournament>> GetAllAsync()
        {
            return await _entities
                .Include(t => t.Groups)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball tournaments by status
        /// </summary>
        /// <param name="status">The tournament status to filter by</param>
        /// <returns>A collection of tournaments with the specified status</returns>
        public async Task<IEnumerable<FloorballTournament>> GetByStatusAsync(FloorballTournamentStatus status)
        {
            return await _entities
                .Include(t => t.Groups)
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new floorball tournament
        /// </summary>
        /// <param name="tournament">The tournament to add</param>
        public override async Task AddAsync(FloorballTournament tournament)
        {
            await base.AddAsync(tournament);
        }

        /// <summary>
        /// Updates an existing floorball tournament
        /// </summary>
        /// <param name="tournament">The tournament to update</param>
        public override async Task UpdateAsync(FloorballTournament tournament)
        {
            await base.UpdateAsync(tournament);
        }

        /// <summary>
        /// Deletes a floorball tournament by ID
        /// </summary>
        /// <param name="id">The ID of the tournament to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballTournament? tournament = await _entities.FindAsync(id);
            if (tournament != null)
            {
                await DeleteAsync(tournament);
            }
        }

        /// <summary>
        /// Checks if a floorball tournament exists
        /// </summary>
        /// <param name="id">The tournament ID</param>
        /// <returns>True if the tournament exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(t => t.Id == id);
        }
    }
}
