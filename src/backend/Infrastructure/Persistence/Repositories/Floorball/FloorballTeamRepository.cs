using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Enums.Common;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation of the floorball team repository
    /// </summary>
    public class FloorballTeamRepository : RepositoryBase<FloorballTeam, FloorballDbContext>, IFloorballTeamRepository
    {
        /// <summary>
        /// Initializes a new instance of the FloorballTeamRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FloorballTeamRepository(FloorballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a floorball team by ID
        /// </summary>
        /// <param name="id">The team ID</param>
        /// <returns>The team if found, null otherwise</returns>
        public async Task<FloorballTeam?> GetByIdAsync(Guid? id)
        {
            // Note: Club relationship is managed at the application level since
            // Club is in a different DbContext (CommonDbContext)
            return await _entities
                .Include(t => t.Roster)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Gets all floorball teams
        /// </summary>
        /// <returns>A collection of all floorball teams</returns>
        public override async Task<IEnumerable<FloorballTeam>> GetAllAsync()
        {
            return await _entities
                .Include(t => t.Roster)
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated floorball teams with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="clubId">Optional club ID filter</param>
        /// <param name="division">Optional division filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball teams</returns>
        public async Task<PagedResult<FloorballTeam>> GetPagedAsync(
            int page, 
            int pageSize,
            string searchTerm = "",
            Guid? clubId = null, 
            Guid? divisionId = null,    
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballTeam> query = _entities.AsQueryable();

            // Apply filters
            if (clubId.HasValue)
            {
                query = query.Where(t => t.ClubId == clubId.Value);
            }

            if (divisionId.HasValue)
            {
                query = query.Where(t => t.DivisionId == divisionId);
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
            List<FloorballTeam> items = await query
                .Include(t => t.Roster)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets the total count of floorball teams with filtering
        /// </summary>
        /// <param name="clubId">Optional club ID filter</param>
        /// <param name="division">Optional division filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of matching floorball teams</returns>
        public async Task<int> GetCountAsync(
            Guid? clubId = null, 
            Guid? divisionId = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballTeam> query = _entities.AsQueryable();

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
        public async Task<IEnumerable<FloorballTeam>> GetByDivisionAsync(Guid divisionId)
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
        public async Task<IEnumerable<FloorballTeam?>> GetByClubIdAsync(Guid clubId)
        {
            // Use the explicit ClubId property for filtering
            IEnumerable<FloorballTeam> teams = await _entities
                .Where(t => t.ClubId == clubId)
                .ToListAsync();
            
            return teams.Cast<FloorballTeam?>();
        }

        /// <summary>
        /// Gets floorball teams where a specific player is in the roster
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>A collection of floorball teams containing the player</returns>
        public async Task<IEnumerable<FloorballTeam>> GetByPlayerIdAsync(Guid playerId)
        {
            return await _entities
                .Include(t => t.Roster)
                .Where(t => t.Roster.Any(r => r.PlayerId == playerId))
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, FloorballTeam>> GetTeamsByPlayerIdsAsync(IEnumerable<Guid> playerIds, CancellationToken cancellationToken = default)
        {
            if (!playerIds.Any())
            {
                return new Dictionary<Guid, FloorballTeam>();
            }

            // Find all teams that contain any of the players
            List<FloorballTeam> teamsWithPlayers = await _entities
                .Include(t => t.Roster)
                .Where(t => t.Roster.Any(p => playerIds.Contains(p.PlayerId)))
                .ToListAsync(cancellationToken);

            Dictionary<Guid, FloorballTeam> playerTeamMap = new Dictionary<Guid, FloorballTeam>();

            // Map each player to their team
            foreach (FloorballTeam team in teamsWithPlayers)
            {
                foreach (FloorballTeamPlayer player in team.Roster)
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
        public async Task<IEnumerable<FloorballTeam>> GetByCompetitionIdAsync(Guid competitionId)
        {
            FloorballCompetition? competition = await _dbContext.FloorballCompetitions
                .Include(s => s.Teams)
                .FirstOrDefaultAsync(s => s.Id == competitionId);

            return competition?.Teams ?? new List<FloorballTeam>();
        }

        /// <summary>
        /// Gets the team standings for a competition
        /// </summary>
        /// <param name="competitionId">The competition ID</param>
        /// <returns>Teams ordered by their standing in the competition</returns>
        public async Task<IEnumerable<FloorballTeam>> GetStandingsAsync(Guid competitionId)
        {
            List<FloorballMatch> matches = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.CompetitionId == competitionId && m.Status == FloorballMatchStatus.Completed)
                .ToListAsync();

            IEnumerable<FloorballTeam> teams = await GetByCompetitionIdAsync(competitionId);
            List<FloorballTeam> teamList = teams.ToList();

            // Calculate points for each team
            Dictionary<Guid, int> teamPoints = new Dictionary<Guid, int>();
            Dictionary<Guid, int> teamGoalDifference = new Dictionary<Guid, int>();

            foreach (FloorballTeam team in teamList)
            {
                teamPoints[team.Id] = 0;
                teamGoalDifference[team.Id] = 0;
            }

            foreach (FloorballMatch match in matches)
            {
                // Standings only count matches where both participants are known. Skip placeholder
                // entries gracefully so future fixtures don't crash this query.
                if (!match.HomeTeamId.HasValue || !match.AwayTeamId.HasValue)
                    continue;

                Guid homeId = match.HomeTeamId.Value;
                Guid awayId = match.AwayTeamId.Value;

                // Home team won
                if (match.HomeScore > match.AwayScore)
                {
                    teamPoints[homeId] += 3;
                }
                // Away team won
                else if (match.AwayScore > match.HomeScore)
                {
                    teamPoints[awayId] += 3;
                }
                // Draw
                else
                {
                    teamPoints[homeId] += 1;
                    teamPoints[awayId] += 1;
                }

                // Update goal difference
                teamGoalDifference[homeId] += match.HomeScore - match.AwayScore;
                teamGoalDifference[awayId] += match.AwayScore - match.HomeScore;
            }

            // Sort teams by points and goal difference
            return teamList
                .OrderByDescending(t => teamPoints.GetValueOrDefault(t.Id, 0))
                .ThenByDescending(t => teamGoalDifference.GetValueOrDefault(t.Id, 0));
        }

        /// <summary>
        /// Gets a team by name
        /// </summary>
        /// <param name="name">The team name</param>
        /// <returns>The team if found, null otherwise</returns>
        public async Task<FloorballTeam?> GetByNameAsync(string name)
        {
            return await _entities
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        /// <summary>
        /// Adds a new team
        /// </summary>
        /// <param name="team">The team to add</param>
        public async override Task AddAsync(FloorballTeam team)
        {
            await _entities.AddAsync(team);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing team
        /// </summary>
        /// <param name="team">The team to update</param>
        public override async Task UpdateAsync(FloorballTeam team)
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
            FloorballTeam? team = await GetByIdAsync(id);
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
        public async Task<IEnumerable<FloorballTeam>> SearchByNameAsync(string searchTerm, int count, CancellationToken cancellationToken = default)
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
        /// Checks if a team with the given name exists
        /// </summary>
        /// <param name="name">The team name</param>
        /// <returns>True if a team with the name exists, false otherwise</returns>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _entities.AnyAsync(t => t.Name == name);
        }

        /// <summary>
        /// Gets floorball teams by player ID
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>A collection of floorball teams the player is in</returns>
        public async Task<IEnumerable<FloorballTeam>> GetTeamsByPlayerIdAsync(Guid playerId)
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
        /// <returns>A collection of matching floorball teams</returns>
        public async Task<IEnumerable<FloorballTeam>> GetByNameFilterAsync(string? nameFilter, CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballTeam> query = _entities;

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
        /// Gets paginated floorball teams without roster with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="searchTerm">Optional search term to filter by team name</param>
        /// <param name="teamCategory">Optional team category filter (Adult, Youth, Women)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of floorball teams without roster</returns>
        public async Task<PagedResult<FloorballTeam>> GetAllTeamsWithoutRosterAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            TeamCategory? teamCategory = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballTeam> query = _entities.AsQueryable();

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
            List<FloorballTeam> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }
    }
} 
