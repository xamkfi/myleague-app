using Domain.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Teams;
using Domain.Enums.Common;
using Domain.Repositories.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football
{
    /// <summary>
    /// Implementation of the football team repository
    /// </summary>
    public class FootballTeamRepository : RepositoryBase<FootballTeam, FootballDbContext>, IFootballTeamRepository
    {
        /// <summary>
        /// Initializes a new instance of the FootballTeamRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FootballTeamRepository(FootballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a football team by ID
        /// </summary>
        /// <param name="id">The team ID</param>
        /// <returns>The team if found, null otherwise</returns>
        public async Task<FootballTeam?> GetByIdAsync(Guid? id)
        {
            // Note: Club relationship is managed at the application level since
            // Club is in a different DbContext (CommonDbContext)
            return await _entities
                .Include(t => t.Roster)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Gets all football teams
        /// </summary>
        /// <returns>A collection of all football teams</returns>
        public override async Task<IEnumerable<FootballTeam>> GetAllAsync()
        {
            return await _entities
                .Include(t => t.Roster)
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated football teams with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="clubId">Optional club ID filter</param>
        /// <param name="division">Optional division filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of football teams</returns>
        public async Task<PagedResult<FootballTeam>> GetPagedAsync(
            int page,
            int pageSize,
            string searchTerm = "",
            Guid? clubId = null,
            Guid? divisionId = null,
            IReadOnlyCollection<Domain.Enums.Common.TeamCategory>? teamCategories = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballTeam> query = _entities.AsQueryable();

            // Apply filters
            if (clubId.HasValue)
            {
                query = query.Where(t => t.ClubId == clubId.Value);
            }

            if (divisionId.HasValue)
            {
                query = query.Where(t => t.DivisionId == divisionId);
            }

            if (teamCategories is { Count: > 0 })
            {
                query = query.Where(t => teamCategories.Contains(t.TeamCategory));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string loweredSearchTerm = searchTerm.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(loweredSearchTerm));
            }

            // Apply ordering by name
            query = query.OrderBy(t => t.Name);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and include roster
            List<FootballTeam> items = await query
                .Include(t => t.Roster)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets the total count of football teams with filtering
        /// </summary>
        /// <param name="clubId">Optional club ID filter</param>
        /// <param name="division">Optional division filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of matching football teams</returns>
        public async Task<int> GetCountAsync(
            Guid? clubId = null,
            Guid? divisionId = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballTeam> query = _entities.AsQueryable();

            // Apply filters
            if (clubId.HasValue)
            {
                query = query.Where(t => t.ClubId == clubId.Value);
            }

            if (divisionId.HasValue)
            {
                query = query.Where(t => t.DivisionId == divisionId.Value);
            }

            return await query.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Gets teams by division
        /// </summary>
        /// <param name="divisionId">The division to filter by</param>
        /// <returns>A collection of teams in the specified division</returns>
        public async Task<IEnumerable<FootballTeam>> GetByDivisionAsync(Guid divisionId)
        {
            return await _entities
                .Where(t => t.DivisionId == divisionId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets teams by club ID
        /// </summary>
        /// <param name="clubId">The club ID to filter by</param>
        /// <returns>A collection of teams belonging to the specified club</returns>
        public async Task<IEnumerable<FootballTeam?>> GetByClubIdAsync(Guid clubId)
        {
            // Use the explicit ClubId property for filtering
            IEnumerable<FootballTeam> teams = await _entities
                .Where(t => t.ClubId == clubId)
                .ToListAsync();

            return teams.Cast<FootballTeam?>();
        }

        /// <summary>
        /// Gets football teams where a specific player is in the roster
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>A collection of football teams containing the player</returns>
        public async Task<IEnumerable<FootballTeam>> GetByPlayerIdAsync(Guid playerId)
        {
            return await _entities
                .Include(t => t.Roster)
                .Where(t => t.Roster.Any(r => r.PlayerId == playerId))
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, FootballTeam>> GetTeamsByPlayerIdsAsync(IEnumerable<Guid> playerIds, CancellationToken cancellationToken = default)
        {
            if (!playerIds.Any())
            {
                return new Dictionary<Guid, FootballTeam>();
            }

            // Find all teams that contain any of the players
            List<FootballTeam> teamsWithPlayers = await _entities
                .Include(t => t.Roster)
                .Where(t => t.Roster.Any(p => playerIds.Contains(p.PlayerId)))
                .ToListAsync(cancellationToken);

            Dictionary<Guid, FootballTeam> playerTeamMap = new Dictionary<Guid, FootballTeam>();

            // Map each player to their team
            foreach (FootballTeam team in teamsWithPlayers)
            {
                foreach (FootballTeamPlayer player in team.Roster)
                {
                    // If the player is in our list of searched players and not already mapped, add them
                    if (playerIds.Contains(player.PlayerId) && !playerTeamMap.ContainsKey(player.PlayerId))
                    {
                        playerTeamMap[player.PlayerId] = team;
                    }
                }
            }

            return playerTeamMap;
        }

        /// <summary>
        /// Gets teams participating in a competition
        /// </summary>
        /// <param name="competitionId">The competition ID</param>
        /// <returns>A collection of teams in the competition</returns>
        public async Task<IEnumerable<FootballTeam>> GetByCompetitionIdAsync(Guid competitionId)
        {
            FootballCompetition? competition = await _dbContext.FootballCompetitions
                .Include(s => s.Teams)
                .FirstOrDefaultAsync(s => s.Id == competitionId);

            return competition?.Teams ?? new List<FootballTeam>();
        }

        /// <summary>
        /// Gets a team by name
        /// </summary>
        /// <param name="name">The team name</param>
        /// <returns>The team if found, null otherwise</returns>
        public async Task<FootballTeam?> GetByNameAsync(string name)
        {
            return await _entities
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        /// <summary>
        /// Adds a new team
        /// </summary>
        /// <param name="team">The team to add</param>
        public async override Task AddAsync(FootballTeam team)
        {
            await _entities.AddAsync(team);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing team
        /// </summary>
        /// <param name="team">The team to update</param>
        public override async Task UpdateAsync(FootballTeam team)
        {
            _entities.Update(team);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a team
        /// </summary>
        /// <param name="id">The ID of the team to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FootballTeam? team = await GetByIdAsync(id);
            if (team != null)
            {
                _entities.Remove(team);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Searches for teams by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="count">The maximum number of results to return.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of teams matching the search term</returns>
        public async Task<IEnumerable<FootballTeam>> SearchByNameAsync(string searchTerm, int count, CancellationToken cancellationToken = default)
        {
            string lowercasedTerm = searchTerm.ToLower();
            return await _entities
                .Where(t => t.Name.ToLower().Contains(lowercasedTerm))
                .OrderBy(t => t.Name)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Checks if a team exists
        /// </summary>
        /// <param name="id">The team ID</param>
        /// <returns>True if the team exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(t => t.Id == id);
        }

        /// <summary>
        /// Gets football teams by player ID
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>A collection of football teams the player is in</returns>
        public async Task<IEnumerable<FootballTeam>> GetTeamsByPlayerIdAsync(Guid playerId)
        {
            return await _entities
                .Include(t => t.Roster)
                .Where(t => t.Roster.Any(r => r.PlayerId == playerId))
                .ToListAsync();
        }

        /// <summary>
        /// Gets teams filtered by name (case-insensitive, partial match)
        /// </summary>
        /// <param name="nameFilter">Optional name filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of matching football teams</returns>
        public async Task<IEnumerable<FootballTeam>> GetByNameFilterAsync(string? nameFilter, CancellationToken cancellationToken = default)
        {
            IQueryable<FootballTeam> query = _entities;

            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                string loweredFilter = nameFilter.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(loweredFilter));
            }

            return await query
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets paginated football teams without roster with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="searchTerm">Optional search term to filter by team name</param>
        /// <param name="teamCategory">Optional team category filter (Adult, Youth, Women)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of football teams without roster</returns>
        public async Task<PagedResult<FootballTeam>> GetAllTeamsWithoutRosterAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            TeamCategory? teamCategory = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballTeam> query = _entities.AsQueryable();

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string loweredSearchTerm = searchTerm.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(loweredSearchTerm));
            }

            // Apply team category filter
            if (teamCategory.HasValue)
            {
                query = query.Where(t => t.TeamCategory == teamCategory.Value);
            }

            // Apply ordering by name
            query = query.OrderBy(t => t.Name);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination - NOTE: Roster is NOT included for performance
            List<FootballTeam> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        public async Task<bool> HasAnyForClubAsync(Guid clubId, CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(t => t.ClubId == clubId, cancellationToken);
        }

        public async Task<bool> HasAnyForDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(t => t.DivisionId == divisionId, cancellationToken);
        }
    }
}
