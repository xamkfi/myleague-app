using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball player repository
    /// </summary>
    public class FloorballPlayerRepository : RepositoryBase<FloorballPlayer, FloorballDbContext>, IFloorballPlayerRepository
    {
        private readonly IPersonRepository _personRepository;

        public FloorballPlayerRepository(FloorballDbContext dbContext, IPersonRepository personRepository) : base(dbContext)
        {
            _personRepository = personRepository;
        }

        private async Task<IQueryable<FloorballPlayer>> ApplyPersonNameSearchAsync(
            IQueryable<FloorballPlayer> query,
            string? searchTerm,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return query;
            }

            IReadOnlyList<Guid> personIds = await _personRepository.GetIdsByNameContainsAsync(searchTerm, cancellationToken);
            if (personIds.Count == 0)
            {
                return query.Where(player => false);
            }

            return query.Where(player => personIds.Contains(player.PersonId));
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
        /// Gets a floorball player by Person ID
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>The player if found, null otherwise</returns>
        public async Task<FloorballPlayer?> GetByPersonIdAsync(Guid personId)
        {
            return await _entities
                .FirstOrDefaultAsync(p => p.PersonId == personId);
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
        /// Gets paginated floorball players with their current team information
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="position">Optional position filter</param>
        /// <param name="teamId">Optional team ID filter</param>
        /// <param name="searchTerm">Optional search term for player names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball players with team information</returns>
        public async Task<PagedResult<(FloorballPlayer Player, FloorballTeam? Team)>> GetPagedWithTeamsAsync(
            int page, 
            int pageSize, 
            bool? isActive = null,
            FloorballPosition? position = null,
            Guid? teamId = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballPlayer> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (position.HasValue)
            {
                // Implement the same logic as Position.CanPlayInPosition method
                if (position.Value == FloorballPosition.None)
                {
                    // None position should not match anything
                    query = query.Where(p => false);
                }
                else if (position.Value == FloorballPosition.Goalkeeper)
                {
                    // For goalkeeper, check CanPlayAsGoalkeeper capability
                    query = query.Where(p => p.Position.CanPlayAsGoalkeeper);
                }
                else
                {
                    // For other positions, check primary OR secondary position
                    query = query.Where(p => p.Position.PrimaryPosition == position.Value || 
                                            p.Position.SecondaryPosition == position.Value);
                }
            }

            if (teamId.HasValue)
            {
                // Get team roster first
                FloorballTeam? team = await _dbContext.FloorballTeams
                    .Include(t => t.Roster)
                    .FirstOrDefaultAsync(t => t.Id == teamId.Value, cancellationToken);

                if (team?.Roster != null)
                {
                    List<Guid> playerIds = team.Roster.Select(r => r.PlayerId).ToList();
                    query = query.Where(p => playerIds.Contains(p.Id));
                }
                else
                {
                    // No team found or no roster, return empty result
                    return PagedResult.Create(new List<(FloorballPlayer Player, FloorballTeam? Team)>(), 0, page, pageSize);
                }
            }

            query = await ApplyPersonNameSearchAsync(query, searchTerm, cancellationToken);

            // Apply ordering by player ID since Person properties are not available
            query = query.OrderBy(p => p.Id);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FloorballPlayer> players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Get team information for each player
            List<(FloorballPlayer Player, FloorballTeam? Team)> playersWithTeams = new List<(FloorballPlayer, FloorballTeam?)>();
            
            foreach (FloorballPlayer player in players)
            {
                // Find the most recent active team for this player
                FloorballTeam? currentTeam = await _dbContext.FloorballTeams
                    .Include(t => t.Roster)
                    .Where(t => t.Roster.Any(r => r.PlayerId == player.Id && r.IsActive))
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                playersWithTeams.Add((player, currentTeam));
            }

            return PagedResult.Create(playersWithTeams, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets paginated floorball players with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="position">Optional position filter</param>
        /// <param name="teamId">Optional team ID filter</param>
        /// <param name="searchTerm">Optional search term for player names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball players</returns>
        public async Task<PagedResult<FloorballPlayer>> GetPagedAsync(
            int page, 
            int pageSize, 
            bool? isActive = null,
            FloorballPosition? position = null,
            Guid? teamId = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballPlayer> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (position.HasValue)
            {
                // Implement the same logic as Position.CanPlayInPosition method
                if (position.Value == FloorballPosition.None)
                {
                    // None position should not match anything
                    query = query.Where(p => false);
                }
                else if (position.Value == FloorballPosition.Goalkeeper)
                {
                    // For goalkeeper, check CanPlayAsGoalkeeper capability
                    query = query.Where(p => p.Position.CanPlayAsGoalkeeper);
                }
                else
                {
                    // For other positions, check primary OR secondary position
                    query = query.Where(p => p.Position.PrimaryPosition == position.Value || 
                                            p.Position.SecondaryPosition == position.Value);
                }
            }

            if (teamId.HasValue)
            {
                // Get team roster first
                FloorballTeam? team = await _dbContext.FloorballTeams
                    .Include(t => t.Roster)
                    .FirstOrDefaultAsync(t => t.Id == teamId.Value, cancellationToken);

                if (team?.Roster != null)
                {
                    List<Guid> playerIds = team.Roster.Select(r => r.PlayerId).ToList();
                    query = query.Where(p => playerIds.Contains(p.Id));
                }
                else
                {
                    // No team found or no roster, return empty result
                    return PagedResult.Create(new List<FloorballPlayer>(), 0, page, pageSize);
                }
            }

            query = await ApplyPersonNameSearchAsync(query, searchTerm, cancellationToken);

            // Apply ordering by player ID since Person properties are not available
            query = query.OrderBy(p => p.Id);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FloorballPlayer> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets the total count of floorball players with filtering
        /// </summary>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="position">Optional position filter</param>
        /// <param name="teamId">Optional team ID filter</param>
        /// <param name="searchTerm">Optional search term for player names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of matching floorball players</returns>
        public async Task<int> GetCountAsync(
            bool? isActive = null,
            FloorballPosition? position = null,
            Guid? teamId = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballPlayer> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (position.HasValue)
            {
                // Implement the same logic as Position.CanPlayInPosition method
                if (position.Value == FloorballPosition.None)
                {
                    // None position should not match anything
                    query = query.Where(p => false);
                }
                else if (position.Value == FloorballPosition.Goalkeeper)
                {
                    // For goalkeeper, check CanPlayAsGoalkeeper capability
                    query = query.Where(p => p.Position.CanPlayAsGoalkeeper);
                }
                else
                {
                    // For other positions, check primary OR secondary position
                    query = query.Where(p => p.Position.PrimaryPosition == position.Value || 
                                            p.Position.SecondaryPosition == position.Value);
                }
            }

            if (teamId.HasValue)
            {
                // Get team roster first
                FloorballTeam? team = await _dbContext.FloorballTeams
                    .Include(t => t.Roster)
                    .FirstOrDefaultAsync(t => t.Id == teamId.Value, cancellationToken);

                if (team?.Roster != null)
                {
                    List<Guid> playerIds = team.Roster.Select(r => r.PlayerId).ToList();
                    query = query.Where(p => playerIds.Contains(p.Id));
                }
                else
                {
                    // No team found or no roster, return 0
                    return 0;
                }
            }

            query = await ApplyPersonNameSearchAsync(query, searchTerm, cancellationToken);

            return await query.CountAsync(cancellationToken);
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
            IQueryable<FloorballPlayer> query = _entities.Where(p => p.IsActive);

            // Implement the same logic as Position.CanPlayInPosition method
            if (position == FloorballPosition.None)
            {
                // None position should not match anything
                query = query.Where(p => false);
            }
            else if (position == FloorballPosition.Goalkeeper)
            {
                // For goalkeeper, check CanPlayAsGoalkeeper capability
                query = query.Where(p => p.Position.CanPlayAsGoalkeeper);
            }
            else
            {
                // For other positions, check primary OR secondary position
                query = query.Where(p => p.Position.PrimaryPosition == position || 
                                        p.Position.SecondaryPosition == position);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets top scorers for the specified competition
        /// </summary>
        /// <param name="competitionId">The competition ID</param>
        /// <param name="count">Maximum number of players to return</param>
        /// <returns>A collection of top scoring players in the competition</returns>
        public async Task<IEnumerable<FloorballPlayer>> GetTopScorersAsync(Guid competitionId, int count = 10)
        {
            List<FloorballMatch> matches = await _dbContext.FloorballMatches
                .Where(m => m.CompetitionId == competitionId && m.Status == FloorballMatchStatus.Completed)
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
        /// Gets top assisters for the specified competition
        /// </summary>
        /// <param name="competitionId">The competition ID</param>
        /// <param name="count">Maximum number of players to return</param>
        /// <returns>A collection of top assisting players in the competition</returns>
        public async Task<IEnumerable<FloorballPlayer>> GetTopAssistersAsync(Guid competitionId, int count = 10)
        {
            List<FloorballMatch> matches = await _dbContext.FloorballMatches
                .Where(m => m.CompetitionId == competitionId && m.Status == FloorballMatchStatus.Completed)
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

        public async Task<Dictionary<Guid, FloorballPlayer>> GetByPersonIdsAsync(IEnumerable<Guid> personIds, CancellationToken cancellationToken = default)
        {
            if (!personIds.Any())
            {
                return new Dictionary<Guid, FloorballPlayer>();
            }

            return await _entities
                .Where(fp => personIds.Contains(fp.PersonId))
                .GroupBy(fp => fp.PersonId)
                .ToDictionaryAsync(g => g.Key, g => g.First(), cancellationToken);
        }

        /// <summary>
        /// Searches for floorball players by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>A collection of floorball players matching the search term</returns>
        public async Task<IEnumerable<FloorballPlayer>> SearchByNameAsync(string searchTerm)
        {
            IQueryable<FloorballPlayer> query = await ApplyPersonNameSearchAsync(_entities.AsQueryable(), searchTerm, CancellationToken.None);
            return await query.ToListAsync();
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

        public async Task<bool> HasCompetitionHistoryAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            bool hasRosterGames = await _dbContext.FloorballTeamPlayers
                .AnyAsync(tp => tp.PlayerId == playerId && tp.GamesPlayed > 0, cancellationToken);
            if (hasRosterGames)
            {
                return true;
            }

            bool hasSeasonStats = await _dbContext.FloorballPlayerSeasonStatistics
                .AnyAsync(s => s.PlayerId == playerId, cancellationToken)
                || await _dbContext.FloorballGoalieSeasonStatistics
                    .AnyAsync(s => s.PlayerId == playerId, cancellationToken);
            if (hasSeasonStats)
            {
                return true;
            }

            bool hasEvents = await _dbContext.FloorballGoals
                    .AnyAsync(g =>
                        g.ScoringPlayerId == playerId
                        || g.AssistingPlayerId == playerId
                        || g.SecondaryAssistingPlayerId == playerId, cancellationToken)
                || await _dbContext.FloorballPenalties
                    .AnyAsync(p => p.PlayerId == playerId, cancellationToken)
                || await _dbContext.FloorballSaves
                    .AnyAsync(s => s.GoalieId == playerId, cancellationToken);
            if (hasEvents)
            {
                return true;
            }

            return await _dbContext.FloorballMatchActivePlayers
                .AnyAsync(
                    ap => ap.PlayerId == playerId
                        && _dbContext.FloorballMatches.Any(m =>
                            m.Id == ap.MatchId && m.Status != FloorballMatchStatus.Scheduled),
                    cancellationToken);
        }
    }
} 
