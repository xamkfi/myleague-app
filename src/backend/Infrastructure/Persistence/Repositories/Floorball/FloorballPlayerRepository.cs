using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball player repository
    /// </summary>
    public class FloorballPlayerRepository : RepositoryBase<FloorballPlayer>, IFloorballPlayerRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballPlayerRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballPlayerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball player by ID
        /// </summary>
        /// <param name="id">The player ID</param>
        /// <returns>The player if found, null otherwise</returns>
        public override async Task<FloorballPlayer?> GetByIdAsync(Guid id)
        {
            return await _entities
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Gets all floorball players
        /// </summary>
        /// <returns>A collection of all floorball players</returns>
        public override async Task<IEnumerable<FloorballPlayer>> GetAllAsync()
        {
            return await _entities
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball players by team ID
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>A collection of floorball players in the team</returns>
        public async Task<IEnumerable<FloorballPlayer>> GetByTeamIdAsync(Guid teamId)
        {
            // Get the team and its roster
            FloorballTeam? team = await _dbContext.FloorballTeams
                .Include(t => t.Roster)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null || team.Roster == null)
                return new List<FloorballPlayer>();

            // Get the player IDs from the roster
            List<Guid> playerIds = team.Roster
                .Select(r => r.PlayerId)
                .ToList();

            // Return the players
            return await _entities
                .Where(p => playerIds.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Gets active floorball players by position
        /// </summary>
        /// <param name="position">The position to filter by</param>
        /// <returns>A collection of active floorball players playing the specified position</returns>
        public async Task<IEnumerable<FloorballPlayer>> GetActiveByPositionAsync(FloorballPosition position)
        {
            return await _entities
                .Where(p => p.Position.PrimaryPosition == position && p.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Gets top scorers for the specified season
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <param name="count">Maximum number of players to return</param>
        /// <returns>A collection of top scoring players in the season</returns>
        public async Task<IEnumerable<FloorballPlayer>> GetTopScorersAsync(Guid seasonId, int count = 10)
        {
            // Get all matches for the season
            List<FloorballMatch> matches = await _dbContext.FloorballMatches
                .Where(m => m.SeasonId == seasonId && m.Status == FloorballMatchStatus.Completed)
                .ToListAsync();

            // Get all player IDs from these matches
            List<Guid> playerIds = matches
                .SelectMany(m => m.GoalEvents)
                .Where(g => g.ScoringPlayerId.HasValue)
                .Select(g => g.ScoringPlayerId!.Value)
                .Distinct()
                .ToList();

            // Get the actual player entities
            List<FloorballPlayer> players = await _entities
                .Where(p => playerIds.Contains(p.Id))
                .ToListAsync();

            // Calculate goal counts
            Dictionary<Guid, int> playerGoals = new Dictionary<Guid, int>();
            foreach (FloorballPlayer player in players)
            {
                int goalCount = matches
                    .SelectMany(m => m.GoalEvents)
                    .Count(g => g.ScoringPlayerId.HasValue && g.ScoringPlayerId.Value == player.Id);
                
                playerGoals[player.Id] = goalCount;
            }

            // Return players sorted by goal count
            return players
                .OrderByDescending(p => playerGoals.GetValueOrDefault(p.Id, 0))
                .Take(count);
        }

        /// <summary>
        /// Gets top assisters for the specified season
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <param name="count">Maximum number of players to return</param>
        /// <returns>A collection of top assisting players in the season</returns>
        public async Task<IEnumerable<FloorballPlayer>> GetTopAssistersAsync(Guid seasonId, int count = 10)
        {
            // Get all matches for the season
            List<FloorballMatch> matches = await _dbContext.FloorballMatches
                .Where(m => m.SeasonId == seasonId && m.Status == FloorballMatchStatus.Completed)
                .ToListAsync();

            // Get all player IDs from these matches
            List<Guid> playerIds = matches
                .SelectMany(m => m.GoalEvents)
                .Where(g => g.AssistingPlayerId.HasValue)
                .Select(g => g.AssistingPlayerId!.Value)
                .Distinct()
                .ToList();

            // Get the actual player entities
            List<FloorballPlayer> players = await _entities
                .Where(p => playerIds.Contains(p.Id))
                .ToListAsync();

            // Calculate assist counts
            Dictionary<Guid, int> playerAssists = new Dictionary<Guid, int>();
            foreach (FloorballPlayer player in players)
            {
                int assistCount = matches
                    .SelectMany(m => m.GoalEvents)
                    .Count(g => g.AssistingPlayerId.HasValue && g.AssistingPlayerId.Value == player.Id);
                
                playerAssists[player.Id] = assistCount;
            }

            // Return players sorted by assist count
            return players
                .OrderByDescending(p => playerAssists.GetValueOrDefault(p.Id, 0))
                .Take(count);
        }

        /// <summary>
        /// Deletes a floorball player by ID
        /// </summary>
        /// <param name="id">The ID of the player to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballPlayer? player = await _entities.FindAsync(id);
            if (player != null)
            {
                await DeleteAsync(player);
            }
        }

        /// <summary>
        /// Searches for floorball players by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>A collection of floorball players matching the search term</returns>
        public async Task<IEnumerable<FloorballPlayer>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();
                
            return await _entities
                .Include(p => p.Person)
                .Where(p => p.Person.FirstName.Contains(searchTerm) || 
                           p.Person.LastName.Contains(searchTerm))
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a floorball player exists
        /// </summary>
        /// <param name="id">The player ID</param>
        /// <returns>True if the player exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(p => p.Id == id);
        }
    }
} 
