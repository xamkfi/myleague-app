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
        public override async Task<FloorballTeam?> GetByIdAsync(Guid id)
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
            Guid? clubId = null, 
            FloorballDivision? division = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballTeam> query = _entities.AsQueryable();

            // Apply filters
            if (clubId.HasValue)
            {
                query = query.Where(t => t.ClubId == clubId.Value);
            }

            if (division.HasValue)
            {
                query = query.Where(t => t.Division == division.Value);
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
            FloorballDivision? division = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FloorballTeam> query = _entities.AsQueryable();

            // Apply filters
            if (clubId.HasValue)
            {
                query = query.Where(t => t.ClubId == clubId.Value);
            }

            if (division.HasValue)
            {
                query = query.Where(t => t.Division == division.Value);
            }

            return await query.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Gets teams by division
        /// </summary>
        /// <param name="division">The division to filter by</param>
        /// <returns>A collection of teams in the specified division</returns>
        public async Task<IEnumerable<FloorballTeam>> GetByDivisionAsync(FloorballDivision division)
        {
            return await _entities
                .Where(t => t.Division == division)
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
        /// Gets teams participating in a season
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <returns>A collection of teams in the season</returns>
        public async Task<IEnumerable<FloorballTeam>> GetBySeasonIdAsync(Guid seasonId)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .Include(s => s.Teams)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            return season?.Teams ?? new List<FloorballTeam>();
        }

        /// <summary>
        /// Gets the team standings for a season
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <returns>Teams ordered by their standing in the season</returns>
        public async Task<IEnumerable<FloorballTeam>> GetStandingsAsync(Guid seasonId)
        {
            // Get all matches for the season
            List<FloorballMatch> matches = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.SeasonId == seasonId && m.Status == FloorballMatchStatus.Completed)
                .ToListAsync();

            // Get all teams in the season
            IEnumerable<FloorballTeam> teams = await GetBySeasonIdAsync(seasonId);
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
                // Home team won
                if (match.HomeScore > match.AwayScore)
                {
                    teamPoints[match.HomeTeamId] += 3;
                }
                // Away team won
                else if (match.AwayScore > match.HomeScore)
                {
                    teamPoints[match.AwayTeamId] += 3;
                }
                // Draw
                else
                {
                    teamPoints[match.HomeTeamId] += 1;
                    teamPoints[match.AwayTeamId] += 1;
                }

                // Update goal difference
                teamGoalDifference[match.HomeTeamId] += match.HomeScore - match.AwayScore;
                teamGoalDifference[match.AwayTeamId] += match.AwayScore - match.HomeScore;
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
        /// <returns>A collection of teams matching the search term</returns>
        public async Task<IEnumerable<FloorballTeam>> SearchByNameAsync(string searchTerm)
        {
            return await _entities
                .Where(t => t.Name.Contains(searchTerm))
                .ToListAsync();
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
    }
} 
