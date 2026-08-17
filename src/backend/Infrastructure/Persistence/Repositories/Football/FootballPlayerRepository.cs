using Domain.Common;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football
{
    /// <summary>
    /// Implementation of the football player repository
    /// </summary>
    public class FootballPlayerRepository : RepositoryBase<FootballPlayer, FootballDbContext>, IFootballPlayerRepository
    {
        private readonly IPersonRepository _personRepository;

        public FootballPlayerRepository(FootballDbContext dbContext, IPersonRepository personRepository) : base(dbContext)
        {
            _personRepository = personRepository;
        }

        private async Task<IQueryable<FootballPlayer>> ApplyPersonNameSearchAsync(
            IQueryable<FootballPlayer> query,
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
        /// Gets a football player by ID
        /// </summary>
        /// <param name="id">The player ID</param>
        /// <returns>The player if found, null otherwise</returns>
        public override async Task<FootballPlayer?> GetByIdAsync(Guid id)
        {
            return await _entities
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Gets a football player by Person ID
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>The player if found, null otherwise</returns>
        public async Task<FootballPlayer?> GetByPersonIdAsync(Guid personId)
        {
            return await _entities
                .FirstOrDefaultAsync(p => p.PersonId == personId);
        }

        /// <summary>
        /// Gets all football players
        /// </summary>
        /// <returns>A collection of all football players</returns>
        public override async Task<IEnumerable<FootballPlayer>> GetAllAsync()
        {
            return await _entities
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated football players with their current team information
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="position">Optional position filter</param>
        /// <param name="teamId">Optional team ID filter</param>
        /// <param name="searchTerm">Optional search term for player names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of football players with team information</returns>
        public async Task<PagedResult<(FootballPlayer Player, FootballTeam? Team)>> GetPagedWithTeamsAsync(
            int page,
            int pageSize,
            bool? isActive = null,
            FootballPosition? position = null,
            Guid? teamId = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballPlayer> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (position.HasValue)
            {
                if (position.Value == FootballPosition.None)
                {
                    query = query.Where(p => false);
                }
                else
                {
                    query = query.Where(p => p.Position.PrimaryPosition == position.Value);
                }
            }

            if (teamId.HasValue)
            {
                List<Guid> playerIds = await _dbContext.FootballTeamPlayers
                    .Where(tp => tp.TeamId == teamId.Value)
                    .Select(tp => tp.PlayerId)
                    .ToListAsync(cancellationToken);

                if (playerIds.Count == 0)
                {
                    return PagedResult.Create(new List<(FootballPlayer Player, FootballTeam? Team)>(), 0, page, pageSize);
                }

                query = query.Where(p => playerIds.Contains(p.Id));
            }

            query = await ApplyPersonNameSearchAsync(query, searchTerm, cancellationToken);

            // Apply ordering by player ID since Person properties are not available
            query = query.OrderBy(p => p.Id);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FootballPlayer> players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Get team information for each player via FootballTeamPlayer
            List<(FootballPlayer Player, FootballTeam? Team)> playersWithTeams = new List<(FootballPlayer, FootballTeam?)>();

            foreach (FootballPlayer player in players)
            {
                FootballTeam? currentTeam = await (
                    from t in _dbContext.FootballTeams
                    join tp in _dbContext.FootballTeamPlayers on t.Id equals tp.TeamId
                    where tp.PlayerId == player.Id && tp.IsActive
                    orderby t.CreatedAt descending
                    select t
                ).FirstOrDefaultAsync(cancellationToken);

                playersWithTeams.Add((player, currentTeam));
            }

            return PagedResult.Create(playersWithTeams, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets paginated football players with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="position">Optional position filter</param>
        /// <param name="teamId">Optional team ID filter</param>
        /// <param name="searchTerm">Optional search term for player names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of football players</returns>
        public async Task<PagedResult<FootballPlayer>> GetPagedAsync(
            int page,
            int pageSize,
            bool? isActive = null,
            FootballPosition? position = null,
            Guid? teamId = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballPlayer> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (position.HasValue)
            {
                if (position.Value == FootballPosition.None)
                {
                    query = query.Where(p => false);
                }
                else
                {
                    query = query.Where(p => p.Position.PrimaryPosition == position.Value);
                }
            }

            if (teamId.HasValue)
            {
                List<Guid> playerIds = await _dbContext.FootballTeamPlayers
                    .Where(tp => tp.TeamId == teamId.Value)
                    .Select(tp => tp.PlayerId)
                    .ToListAsync(cancellationToken);

                if (playerIds.Count == 0)
                {
                    return PagedResult.Create(new List<FootballPlayer>(), 0, page, pageSize);
                }

                query = query.Where(p => playerIds.Contains(p.Id));
            }

            query = await ApplyPersonNameSearchAsync(query, searchTerm, cancellationToken);

            // Apply ordering by player ID since Person properties are not available
            query = query.OrderBy(p => p.Id);

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            List<FootballPlayer> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets the total count of football players with filtering
        /// </summary>
        /// <param name="isActive">Optional active status filter</param>
        /// <param name="position">Optional position filter</param>
        /// <param name="teamId">Optional team ID filter</param>
        /// <param name="searchTerm">Optional search term for player names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of matching football players</returns>
        public async Task<int> GetCountAsync(
            bool? isActive = null,
            FootballPosition? position = null,
            Guid? teamId = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballPlayer> query = _entities.AsQueryable();

            // Apply filters
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (position.HasValue)
            {
                if (position.Value == FootballPosition.None)
                {
                    query = query.Where(p => false);
                }
                else
                {
                    query = query.Where(p => p.Position.PrimaryPosition == position.Value);
                }
            }

            if (teamId.HasValue)
            {
                List<Guid> playerIds = await _dbContext.FootballTeamPlayers
                    .Where(tp => tp.TeamId == teamId.Value)
                    .Select(tp => tp.PlayerId)
                    .ToListAsync(cancellationToken);

                if (playerIds.Count == 0)
                {
                    return 0;
                }

                query = query.Where(p => playerIds.Contains(p.Id));
            }

            query = await ApplyPersonNameSearchAsync(query, searchTerm, cancellationToken);

            return await query.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Gets football players by team ID
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>A collection of football players in the team</returns>
        public async Task<IEnumerable<FootballPlayer>> GetByTeamIdAsync(Guid teamId)
        {
            List<Guid> playerIds = await _dbContext.FootballTeamPlayers
                .Where(tp => tp.TeamId == teamId)
                .Select(tp => tp.PlayerId)
                .ToListAsync();

            if (playerIds.Count == 0)
                return new List<FootballPlayer>();

            return await _entities
                .Where(p => playerIds.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Gets active football players by position
        /// </summary>
        /// <param name="position">The position to filter by</param>
        /// <returns>A collection of active football players playing the specified position</returns>
        public async Task<IEnumerable<FootballPlayer>> GetActiveByPositionAsync(FootballPosition position)
        {
            IQueryable<FootballPlayer> query = _entities.Where(p => p.IsActive);

            if (position == FootballPosition.None)
            {
                query = query.Where(p => false);
            }
            else
            {
                query = query.Where(p => p.Position.PrimaryPosition == position);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Deletes a football player by ID
        /// </summary>
        /// <param name="id">The ID of the player to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FootballPlayer? player = await _entities.FindAsync(id);
            if (player != null)
            {
                await DeleteAsync(player);
            }
        }

        public async Task<Dictionary<Guid, FootballPlayer>> GetByPersonIdsAsync(IEnumerable<Guid> personIds, CancellationToken cancellationToken = default)
        {
            if (!personIds.Any())
            {
                return new Dictionary<Guid, FootballPlayer>();
            }

            return await _entities
                .Where(fp => personIds.Contains(fp.PersonId))
                .GroupBy(fp => fp.PersonId)
                .ToDictionaryAsync(g => g.Key, g => g.First(), cancellationToken);
        }

        /// <summary>
        /// Searches for football players by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>A collection of football players matching the search term</returns>
        public async Task<IEnumerable<FootballPlayer>> SearchByNameAsync(string searchTerm)
        {
            IQueryable<FootballPlayer> query = await ApplyPersonNameSearchAsync(_entities.AsQueryable(), searchTerm, CancellationToken.None);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Checks if a football player exists
        /// </summary>
        /// <param name="id">The player ID</param>
        /// <returns>True if the player exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(p => p.Id == id);
        }
    }
}
