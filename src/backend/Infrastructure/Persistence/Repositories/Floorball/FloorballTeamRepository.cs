using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

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
            return await _entities
                .Include(t => t.Club)
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
                .Include(t => t.Club)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a floorball team by name
        /// </summary>
        /// <param name="name">The team name</param>
        /// <returns>The team if found, null otherwise</returns>
        public async Task<FloorballTeam?> GetByNameAsync(string name)
        {
            return await _entities
                .Include(t => t.Club)
                .Include(t => t.Roster)
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        /// <summary>
        /// Gets floorball teams by club ID
        /// </summary>
        /// <param name="clubId">The club ID</param>
        /// <returns>A collection of floorball teams belonging to the club</returns>
        public async Task<IEnumerable<FloorballTeam?>> GetByClubIdAsync(Guid clubId)
        {
            return await _entities
                .Include(t => t.Club)
                .Include(t => t.Roster)
                .Where(t => t.Club.Id == clubId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball teams by division
        /// </summary>
        /// <param name="division">The division to filter by</param>
        /// <returns>A collection of floorball teams in the specified division</returns>
        public async Task<IEnumerable<FloorballTeam>> GetByDivisionAsync(FloorballDivision division)
        {
            return await _entities
                .Include(t => t.Club)
                .Where(t => t.Division == division)
                .ToListAsync();
        }

        /// <summary>
        /// Gets floorball teams participating in a season
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <returns>A collection of floorball teams in the season</returns>
        public async Task<IEnumerable<FloorballTeam>> GetBySeasonIdAsync(Guid seasonId)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .Include(s => s.Teams)
                .ThenInclude(t => t.Club)
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
        /// Adds a new floorball team
        /// </summary>
        /// <param name="team">The team to add</param>
        public override async Task AddAsync(FloorballTeam team)
        {
            await base.AddAsync(team);
        }

        /// <summary>
        /// Updates an existing floorball team
        /// </summary>
        /// <param name="team">The team to update</param>
        public override async Task UpdateAsync(FloorballTeam team)
        {
            await base.UpdateAsync(team);
        }

        /// <summary>
        /// Deletes a floorball team by ID
        /// </summary>
        /// <param name="id">The ID of the team to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FloorballTeam? team = await _entities.FindAsync(id);
            if (team != null)
            {
                await DeleteAsync(team);
            }
        }

        /// <summary>
        /// Searches for floorball teams by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>A collection of floorball teams matching the search term</returns>
        public async Task<IEnumerable<FloorballTeam>> SearchByNameAsync(string searchTerm)
        {
            return await _entities
                .Include(t => t.Club)
                .Where(t => t.Name.Contains(searchTerm))
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a floorball team exists
        /// </summary>
        /// <param name="id">The team ID</param>
        /// <returns>True if the team exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(t => t.Id == id);
        }
    }
} 
