using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball match repository
    /// </summary>
    public class FloorballMatchRepository : RepositoryBase<FloorballMatch, FloorballDbContext>, IFloorballMatchRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballMatchRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballMatchRepository(FloorballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball match by ID
        /// </summary>
        /// <param name="id">The match ID</param>
        /// <returns>The match if found, null otherwise</returns>
        public override async Task<FloorballMatch?> GetByIdAsync(Guid id)
        {
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException($"Match with ID {id} not found.");
        }

        /// <summary>
        /// Gets all floorball matches
        /// </summary>
        /// <returns>A collection of all floorball matches</returns>
        public override async Task<IEnumerable<FloorballMatch>> GetAllAsync()
        {
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches for a specified season
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <returns>A collection of matches in the season</returns>
        public async Task<IEnumerable<FloorballMatch>> GetBySeasonIdAsync(Guid seasonId)
        {
            return await _entities
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.SeasonId == seasonId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches for a specified team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>A collection of matches involving the team</returns>
        public async Task<IEnumerable<FloorballMatch>> GetByTeamIdAsync(Guid teamId)
        {
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets upcoming matches for a specified team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="count">Maximum number of matches to return</param>
        /// <returns>A collection of upcoming matches for the team</returns>
        public async Task<IEnumerable<FloorballMatch>> GetUpcomingByTeamIdAsync(Guid teamId, int count = 5)
        {
            DateTime now = DateTime.UtcNow;
            
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                            m.ScheduledDateTime > now &&
                            m.Status == FloorballMatchStatus.Scheduled)
                .OrderBy(m => m.ScheduledDateTime)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Gets past matches for a specified team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="count">Maximum number of matches to return</param>
        /// <returns>A collection of past matches for the team</returns>
        public async Task<IEnumerable<FloorballMatch>> GetPastByTeamIdAsync(Guid teamId, int count = 5)
        {
            DateTime now = DateTime.UtcNow;
            
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                            (m.ScheduledDateTime < now || m.Status == FloorballMatchStatus.Completed))
                .OrderByDescending(m => m.ScheduledDateTime)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches by status
        /// </summary>
        /// <param name="status">The match status</param>
        /// <returns>A collection of matches with the specified status</returns>
        public async Task<IEnumerable<FloorballMatch>> GetByStatusAsync(FloorballMatchStatus status)
        {
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches requiring officials
        /// </summary>
        /// <returns>A collection of matches needing officials</returns>
        public async Task<IEnumerable<FloorballMatch>> GetMatchesNeedingOfficialsAsync()
        {
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .Where(m => m.Status == FloorballMatchStatus.Scheduled && 
                           (m.Officials == null || !m.Officials.Any() || m.Officials.Count < 2))
                .OrderBy(m => m.ScheduledDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches scheduled for a date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>A collection of matches scheduled in the date range</returns>
        public async Task<IEnumerable<FloorballMatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.ScheduledDateTime >= startDate && m.ScheduledDateTime <= endDate)
                .OrderBy(m => m.ScheduledDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches at a specific venue
        /// </summary>
        /// <param name="venue">The venue name</param>
        /// <returns>A collection of matches at the venue</returns>
        public async Task<IEnumerable<FloorballMatch>> GetByVenueAsync(string venue)
        {
            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Venue!.Contains(venue))
                .OrderBy(m => m.ScheduledDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new floorball match
        /// </summary>
        /// <param name="match">The match to add</param>
        public override async Task AddAsync(FloorballMatch match)
        {
            await _entities.AddAsync(match);
        }

        /// <summary>
        /// Updates an existing floorball match
        /// </summary>
        /// <param name="match">The match to update</param>
        public override async Task UpdateAsync(FloorballMatch match)
        {
            _dbContext.Entry(match).State = EntityState.Modified;
        }

        /// <summary>
        /// Deletes a floorball match by ID
        /// </summary>
        /// <param name="id">The ID of the match to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballMatch? match = await _entities.FindAsync(id);
            if (match != null)
            {
                await DeleteAsync(match);
            }
        }

        /// <summary>
        /// Checks if a floorball match exists
        /// </summary>
        /// <param name="id">The match ID</param>
        /// <returns>True if the match exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(m => m.Id == id);
        }
    }
} 
