using Domain.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football
{
    /// <summary>
    /// Implementation of the football match repository
    /// </summary>
    public class FootballMatchRepository : RepositoryBase<FootballMatch, FootballDbContext>, IFootballMatchRepository
    {
        /// <summary>
        /// Initializes a new instance of the FootballMatchRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public FootballMatchRepository(FootballDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a football match by ID
        /// </summary>
        /// <param name="id">The match ID</param>
        /// <returns>The match if found, null otherwise</returns>
        public override async Task<FootballMatch?> GetByIdAsync(Guid id)
        {
            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
                .Include(m => m.Lineup)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException($"Match with ID {id} not found.");
        }

        /// <summary>
        /// Marks a match event as added, so it will be inserted into the database
        /// </summary>
        /// <param name="matchEvent"></param>
        public void MarkEventAsAdded(FootballMatchEvent matchEvent)
        {
            _dbContext.Entry(matchEvent).State = EntityState.Added;
        }

        /// <summary>
        /// Gets all football matches
        /// </summary>
        /// <returns>A collection of all football matches</returns>
        public override async Task<IEnumerable<FootballMatch>> GetAllAsync()
        {
            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated football matches with filtering support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="competitionId">Optional competition ID filter</param>
        /// <param name="teamId">Optional team ID filter (home or away)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="status">Optional match status filter</param>
        /// <param name="sortOrder">Optional sort order ("asc" or "desc")</param>
        /// <param name="searchQuery">Optional search query to filter by team names (case-insensitive, partial match)</param>
        /// <param name="tournamentGroupId">Optional tournament group ID filter (only matches assigned to this tournament group)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of football matches</returns>
        public async Task<PagedResult<FootballMatch>> GetPagedAsync(
            int page,
            int pageSize,
            Guid? competitionId = null,
            Guid? teamId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            FootballMatchStatus? status = null,
            string sortOrder = "desc",
            string? searchQuery = null,
            Guid? tournamentGroupId = null,
            FootballCompetitionType? competitionType = null,
            Domain.Enums.Common.TeamCategory? teamCategory = null,
            CancellationToken cancellationToken = default)
        {
            DateTime? startDateUtc = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : null;
            DateTime? endDateUtc = endDate.HasValue
                ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                : null;

            IQueryable<FootballMatch> query = _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
                .AsQueryable();

            // Apply filters
            if (competitionId.HasValue)
            {
                query = query.Where(m => m.CompetitionId == competitionId.Value);
            }

            if (teamId.HasValue)
            {
                query = query.Where(m => m.HomeTeamId == teamId.Value || m.AwayTeamId == teamId.Value);
            }

            if (startDateUtc.HasValue)
            {
                query = query.Where(m => m.ScheduledDateTime >= startDateUtc.Value);
            }

            if (endDateUtc.HasValue)
            {
                query = query.Where(m => m.ScheduledDateTime <= endDateUtc.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            if (tournamentGroupId.HasValue)
            {
                query = query.Where(m => m.TournamentGroupId == tournamentGroupId.Value);
            }

            if (competitionType.HasValue)
            {
                if (competitionType.Value == FootballCompetitionType.Tournament)
                {
                    query = query.Where(m => m.Competition is FootballTournament);
                }
                else
                {
                    query = query.Where(m => m.Competition is FootballSeason);
                }
            }

            if (teamCategory.HasValue)
            {
                query = query.Where(m => m.Competition.TeamCategory == teamCategory.Value);
            }

            // Apply search query filter (team names)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string searchTerm = searchQuery.Trim().ToLower();
                query = query.Where(m =>
                    m.HomeTeam.Name.ToLower().Contains(searchTerm) ||
                    m.AwayTeam.Name.ToLower().Contains(searchTerm)
                );
            }

            // Apply ordering by scheduled date
            if (sortOrder == "desc")
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
            List<FootballMatch> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <summary>
        /// Gets matches for a specified competition
        /// </summary>
        /// <param name="competitionId">The competition ID</param>
        /// <returns>A collection of matches in the competition</returns>
        public async Task<IEnumerable<FootballMatch>> GetByCompetitionIdAsync(Guid competitionId)
        {
            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
                .Where(m => m.CompetitionId == competitionId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches assigned to a specific tournament group, optionally filtered by status.
        /// </summary>
        public async Task<IEnumerable<FootballMatch>> GetByTournamentGroupAsync(
            Guid tournamentGroupId,
            FootballMatchStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<FootballMatch> query = _entities
                .AsNoTracking()
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.TournamentGroupId == tournamentGroupId);

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            return await query
                .OrderBy(m => m.ScheduledDateTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets matches for a specified team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>A collection of matches involving the team</returns>
        public async Task<IEnumerable<FootballMatch>> GetByTeamIdAsync(Guid teamId)
        {
            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Officials)
                .Include(m => m.PeriodScores)
                .Include(m => m.Events)
                .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets upcoming matches for a specified team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="count">Maximum number of matches to return</param>
        /// <returns>A collection of upcoming matches for the team</returns>
        public async Task<IEnumerable<FootballMatch>> GetUpcomingByTeamIdAsync(Guid teamId, int count = 5)
        {
            DateTime now = DateTime.UtcNow;

            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                            m.ScheduledDateTime > now &&
                            m.Status == FootballMatchStatus.Scheduled)
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
        public async Task<IEnumerable<FootballMatch>> GetPastByTeamIdAsync(Guid teamId, int count = 5)
        {
            DateTime now = DateTime.UtcNow;

            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                            (m.ScheduledDateTime < now || m.Status == FootballMatchStatus.Completed))
                .OrderByDescending(m => m.ScheduledDateTime)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches by status
        /// </summary>
        /// <param name="status">The match status</param>
        /// <returns>A collection of matches with the specified status</returns>
        public async Task<IEnumerable<FootballMatch>> GetByStatusAsync(FootballMatchStatus status)
        {
            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// Gets matches scheduled for a date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>A collection of matches scheduled in the date range</returns>
        public async Task<IEnumerable<FootballMatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            DateTime startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            DateTime endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.ScheduledDateTime >= startUtc && m.ScheduledDateTime <= endUtc)
                .OrderBy(m => m.ScheduledDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new football match
        /// </summary>
        /// <param name="match">The match to add</param>
        public override async Task AddAsync(FootballMatch match)
        {
            await _entities.AddAsync(match);
        }

        /// <summary>
        /// Updates an existing football match
        /// </summary>
        /// <param name="match">The match to update</param>
        public override async Task UpdateAsync(FootballMatch match)
        {
            _dbContext.Entry(match).State = EntityState.Modified;
            // Ensure related PeriodScores modifications are tracked and persisted
            foreach (FootballPeriodScore periodScore in match.PeriodScores)
            {
                _dbContext.Entry(periodScore).State = EntityState.Modified;
            }

            // Ensure Lineup additions/removals are tracked. Existing rows can stay Unchanged
            // because the entity is immutable; new rows get Added by EF when attached via
            // navigation, deletions are handled by Remove().
            foreach (FootballMatchLineupPlayer lineupPlayer in match.Lineup)
            {
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<FootballMatchLineupPlayer> entry = _dbContext.Entry(lineupPlayer);
                if (entry.State == EntityState.Detached)
                {
                    entry.State = EntityState.Added;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a football match by ID
        /// </summary>
        /// <param name="id">The ID of the match to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            FootballMatch? match = await _entities.FindAsync(id);
            if (match != null)
            {
                await DeleteAsync(match);
            }
        }

        /// <inheritdoc />
        public async Task<int> DeleteAllByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
        {
            // Snapshot the match ids once so all the follow-up cleanups operate on the same set,
            // even if something is concurrently inserted (unlikely for a Draft tournament being
            // deleted, but cheap insurance).
            Guid[] matchIds = await _entities
                .AsNoTracking()
                .Where(m => m.CompetitionId == competitionId)
                .Select(m => m.Id)
                .ToArrayAsync(cancellationToken);

            if (matchIds.Length == 0)
            {
                return 0;
            }

            // 1) FootballMatchTeamStatistics has no DB-level FK to FootballMatch (the navigation
            //    is Ignored in the EF configuration so EF never creates one). Without this manual
            //    cleanup the rows would be orphaned after we drop the matches.
            await _dbContext.FootballMatchTeamStatistics
                .Where(s => matchIds.Contains(s.MatchId))
                .ExecuteDeleteAsync(cancellationToken);

            // 2) Match.NextMatchId is a RESTRICT self-reference used by the playoff bracket. Null
            //    it out first so the subsequent bulk DELETE doesn't fail when bracket parents
            //    reference bracket children we're about to remove.
            await _entities
                .Where(m => m.CompetitionId == competitionId && m.NextMatchId != null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(m => m.NextMatchId, _ => (Guid?)null),
                    cancellationToken);

            // 3) Now wipe the matches. The DB cascades to FootballPeriodScores,
            //    FootballMatchEvents (and its TPH subtypes Goals/Cards/Substitutions),
            //    FootballMatchLineupPlayers, and the FootballMatchOfficial join table.
            int deleted = await _entities
                .Where(m => m.CompetitionId == competitionId)
                .ExecuteDeleteAsync(cancellationToken);

            return deleted;
        }

        /// <summary>
        /// Checks if a football match exists
        /// </summary>
        /// <param name="id">The match ID</param>
        /// <returns>True if the match exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<FootballMatch>> GetTodaysMatchesByTeamAsync(Guid teamId, CancellationToken cancellationToken)
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime tomorrow = today.AddDays(1);

            return await _entities
                .Include(m => m.Competition)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                               m.ScheduledDateTime >= today && m.ScheduledDateTime < tomorrow)
                .OrderBy(m => m.ScheduledDateTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieve last completed matches for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="competitionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FootballMatch>> GetLastCompletedByTeamAsync(Guid teamId, Guid? competitionId = null, int count = 5)
        {
            return await _entities
                .AsNoTracking()
                .Include(m => m.Competition)
                .Where(m =>
                    (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                    m.Status == FootballMatchStatus.Completed &&
                    (!competitionId.HasValue || m.CompetitionId == competitionId.Value))
                .OrderByDescending(m => m.ScheduledDateTime)
                .Take(count)
                .ToListAsync();
        }
    }
}
