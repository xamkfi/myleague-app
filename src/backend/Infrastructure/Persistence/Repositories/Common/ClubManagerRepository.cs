using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the club manager (club admin) repository
    /// </summary>
    public class ClubManagerRepository : RepositoryBase<ClubManager, CommonDbContext>, IClubManagerRepository
    {
        /// <summary>
        /// Initializes a new instance of the ClubManagerRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public ClubManagerRepository(CommonDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets all club manager rows for a person
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>All club manager rows for the person</returns>
        public async Task<IEnumerable<ClubManager>> GetAllByPersonIdAsync(Guid personId)
        {
            return await _entities
                .Where(cm => cm.PersonId == personId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all club manager rows for a club
        /// </summary>
        /// <param name="clubId">The club ID</param>
        /// <returns>All club manager rows for the club</returns>
        public async Task<IEnumerable<ClubManager>> GetAllByClubIdAsync(Guid clubId)
        {
            return await _entities
                .Where(cm => cm.ClubId == clubId)
                .ToListAsync();
        }

        /// <summary>
        /// Checks whether a person is an active manager of a specific club
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <param name="clubId">The club ID</param>
        /// <returns>True if an active manager row exists for the person and club</returns>
        public async Task<bool> IsActiveManagerOfClubAsync(Guid personId, Guid clubId)
        {
            return await _entities
                .AnyAsync(cm => cm.PersonId == personId && cm.ClubId == clubId && cm.IsActive);
        }

        /// <summary>
        /// Gets a club manager row for a specific person and club
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <param name="clubId">The club ID</param>
        /// <returns>The club manager row if found, null otherwise</returns>
        public async Task<ClubManager?> GetByPersonAndClubAsync(Guid personId, Guid clubId)
        {
            return await _entities
                .FirstOrDefaultAsync(cm => cm.PersonId == personId && cm.ClubId == clubId);
        }
    }
}
