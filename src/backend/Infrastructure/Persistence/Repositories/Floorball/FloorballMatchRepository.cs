using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

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
                .Include(m => m.PeriodScores)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException($"Match with ID {id} not found.");
        }

        /// <summary>
        /// Marks a match event as added, so it will be inserted into the database
        /// </summary>
        /// <param name="matchEvent"></param>
        public void MarkEventAsAdded(FloorballMatchEvent matchEvent)
        {
            _dbContext.Entry(matchEvent).State = EntityState.Added;
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
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated floorball matches with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="seasonId">Optional season ID filter</param>
        /// <param name="teamId">Optional team ID filter (home or away)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="status">Optional match status filter</param>
        /// <param name="sortOrder">Optional sort order ("asc" or "desc")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball matches</returns>
        public async Task<PagedResult<FloorballMatch>> GetPagedAsync(
            int page, 
            int pageSize, 
            Guid? seasonId = null,
            Guid? teamId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            FloorballMatchStatus? status = null,
            string sortOrder = "desc",
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballMatch> query = _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
                .AsQueryable();

            // Apply filters
            if (seasonId.HasValue)
            {
                query = query.Where(m => m.SeasonId == seasonId.Value);
            }

            if (teamId.HasValue)
            {
                query = query.Where(m => m.HomeTeamId == teamId.Value || m.AwayTeamId == teamId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.ScheduledDateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.ScheduledDateTime <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            // Apply ordering by scheduled date
            if(sortOrder == "desc")
            {
                query = query.OrderByDescending(m => m.ScheduledDateTime);
            }
            else
            {
                query = query.OrderBy(m => m.ScheduledDateTime);
            }
            

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FloorballMatch> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets the total count of floorball matches with filtering
        /// </summary>
        /// <param name="seasonId">Optional season ID filter</param>
        /// <param name="teamId">Optional team ID filter (home or away)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="status">Optional match status filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of matching floorball matches</returns>
        public async Task<int> GetCountAsync(
            Guid? seasonId = null,
            Guid? teamId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            FloorballMatchStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballMatch> query = _entities.AsQueryable();

            // Apply filters
            if (seasonId.HasValue)
            {
                query = query.Where(m => m.SeasonId == seasonId.Value);
            }

            if (teamId.HasValue)
            {
                query = query.Where(m => m.HomeTeamId == teamId.Value || m.AwayTeamId == teamId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.ScheduledDateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.ScheduledDateTime <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            return await query.CountAsync(cancellationToken);
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
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
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
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
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

            await Task.CompletedTask;
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

        public async Task<IEnumerable<FloorballMatch>> GetTodaysMatchesByTeamAsync(Guid teamId, CancellationToken cancellationToken)
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime tomorrow = today.AddDays(1);

            return await _entities
                .Include(m => m.Season)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                               m.ScheduledDateTime >= today && m.ScheduledDateTime < tomorrow)
                .OrderBy(m => m.ScheduledDateTime)
                .ToListAsync(cancellationToken);
        }

    }
} 
