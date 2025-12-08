using Domain.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;
using System.Linq;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the club repository
    /// </summary>
    public class ClubRepository : RepositoryBase<Club, CommonDbContext>, IClubRepository
    {
        /// <summary>
        /// Initializes a new instance of the ClubRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public ClubRepository(CommonDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a club by ID
        /// </summary>
        /// <param name="id">The club ID</param>
        /// <returns>The club if found, null otherwise</returns>
        public override async Task<Club?> GetByIdAsync(Guid id)
        {
            // Note: FloorballTeams and HockeyTeams are in different DbContexts
            // These relationships should be loaded separately by the application layer
            return await _entities
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Dictionary<Guid, Club>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _entities
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c, cancellationToken);
        }

        /// <summary>
        /// Gets a club by name
        /// </summary>
        /// <param name="name">The club name</param>
        /// <returns>The club if found, null otherwise</returns>
        public async Task<Club?> GetByNameAsync(string name)
        {
            return await _entities
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        /// <summary>
        /// Gets all clubs
        /// </summary>
        /// <returns>A collection of all clubs</returns>
        public override async Task<IEnumerable<Club>> GetAllAsync()
        {
            return await _entities
                .ToListAsync();
        }

        /// <summary>
        /// Gets clubs by country
        /// </summary>
        /// <param name="country">The country to filter by</param>
        /// <returns>A collection of clubs in the specified country</returns>
        public async Task<IEnumerable<Club>> GetByCountryAsync(string country)
        {
            return await _entities
                .Where(c => c.Country == country)
                .ToListAsync();
        }

        /// <summary>
        /// Gets clubs by city
        /// </summary>
        /// <param name="city">The city to filter by</param>
        /// <returns>A collection of clubs in the specified city</returns>
        public async Task<IEnumerable<Club>> GetByCityAsync(string city)
        {
            return await _entities
                .Where(c => c.City == city)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new club
        /// </summary>
        /// <param name="club">The club to add</param>
        public async override Task AddAsync(Club club)
        {
            await _entities.AddAsync(club);
        }

        /// <summary>
        /// Updates an existing club
        /// </summary>
        /// <param name="club">The club to update</param>
        public override Task UpdateAsync(Club club)
        {
            _entities.Update(club);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a club
        /// </summary>
        /// <param name="id">The ID of the club to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            Club? club = await GetByIdAsync(id);
            if (club != null)
            {
                _entities.Remove(club);
            }
        }

        /// <summary>
        /// Searches for clubs by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="count">The maximum number of results to return.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of clubs matching the search term</returns>
        public async Task<IEnumerable<Club>> SearchByNameAsync(string searchTerm, int count, CancellationToken cancellationToken = default)
        {
            string lowercasedTerm = searchTerm.ToLower();
            return await _entities
                .Where(c => c.Name.ToLower().Contains(lowercasedTerm))
                .OrderBy(c => c.Name)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Checks if a club exists
        /// </summary>
        /// <param name="id">The club ID</param>
        /// <returns>True if the club exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(c => c.Id == id);
        }

        /// <summary>
        /// Checks if a club with the given name exists
        /// </summary>
        /// <param name="name">The club name</param>
        /// <returns>True if a club with the name exists, false otherwise</returns>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _entities.AnyAsync(c => c.Name == name);
        }

        /// <summary>
        /// Gets a paginated list of clubs ordered by name.
        /// </summary>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of clubs</returns>
        public async Task<PagedResult<Club>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            IQueryable<Club> query = _entities
                .AsNoTracking()
                .OrderBy(c => c.Name);

            int totalCount = await query.CountAsync(cancellationToken);

            List<Club> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }
    }
} 
