using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation for managing floorball season divisions and memberships
    /// </summary>
    public class FloorballSeasonDivisionRepository : IFloorballSeasonDivisionRepository
    {
        private readonly FloorballDbContext _db;

        public FloorballSeasonDivisionRepository(FloorballDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FloorballSeasonDivision>> GetSeasonDivisionsAsync(Guid seasonId)
        {
            return await _db.Set<FloorballSeasonDivision>()
                .Include(sd => sd.Teams)
                .Where(sd => sd.SeasonId == seasonId)
                .ToListAsync();
        }

        public async Task<FloorballSeasonDivision?> GetSeasonDivisionAsync(Guid seasonId, Guid divisionId)
        {
            return await _db.Set<FloorballSeasonDivision>()
                .Include(sd => sd.Teams)
                .FirstOrDefaultAsync(sd => sd.SeasonId == seasonId && sd.DivisionId == divisionId);
        }

        public async Task<IEnumerable<FloorballSeason>> GetSeasonsByDivisionAsync(Guid divisionId)
        {
            HashSet<Guid> seasonIds = await _db.Set<FloorballSeasonDivision>()
                .Where(sd => sd.DivisionId == divisionId)
                .Select(sd => sd.SeasonId)
                .ToHashSetAsync();

            return await _db.FloorballSeasons
                .Include(s => s.Matches)
                .Where(s => seasonIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<FloorballSeason>> GetSeasonsByTeamAsync(Guid teamId)
        {
            HashSet<Guid> seasonIds = await _db.Set<FloorballSeasonDivisionTeam>()
                .Where(sdt => sdt.TeamId == teamId)
                .Select(sdt => sdt.SeasonId)
                .ToHashSetAsync();

            return await _db.FloorballSeasons
                .Include(s => s.Matches)
                .Where(s => seasonIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<FloorballSeasonDivisionTeam>> GetSeasonDivisionTeamsAsync(Guid seasonId)
        {
            return await _db.Set<FloorballSeasonDivisionTeam>()
                .Include(sdt => sdt.Team)
                .Where(sdt => sdt.SeasonId == seasonId)
                .ToListAsync();
        }

        public async Task AddSeasonDivisionAsync(Guid seasonId, Guid divisionId)
        {
            bool exists = await _db.Set<FloorballSeasonDivision>().AnyAsync(sd => sd.SeasonId == seasonId && sd.DivisionId == divisionId);
            if (exists) return;

            _db.Set<FloorballSeasonDivision>().Add(new FloorballSeasonDivision(seasonId, divisionId));
            await _db.SaveChangesAsync();
        }

        public async Task RemoveSeasonDivisionAsync(Guid seasonId, Guid divisionId)
        {
            FloorballSeasonDivision? sd = await _db.Set<FloorballSeasonDivision>()
                .FirstOrDefaultAsync(x => x.SeasonId == seasonId && x.DivisionId == divisionId);
            if (sd == null) return;

            _db.Set<FloorballSeasonDivision>().Remove(sd);
            await _db.SaveChangesAsync();
        }

        public async Task AddTeamToSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId)
        {
            FloorballSeasonDivision? sd = await GetSeasonDivisionAsync(seasonId, divisionId);
            if (sd == null)
            {
                sd = new FloorballSeasonDivision(seasonId, divisionId);
                _db.Set<FloorballSeasonDivision>().Add(sd);
                await _db.SaveChangesAsync();
            }

            bool exists = await _db.Set<FloorballSeasonDivisionTeam>()
                .AnyAsync(x => x.SeasonDivisionId == sd.Id && x.TeamId == teamId);
            if (exists) return;

            _db.Set<FloorballSeasonDivisionTeam>().Add(new FloorballSeasonDivisionTeam(sd.Id, teamId, seasonId));
            await _db.SaveChangesAsync();
        }

        public async Task RemoveTeamFromSeasonDivisionAsync(Guid seasonId, Guid divisionId, Guid teamId)
        {
            FloorballSeasonDivision? sd = await GetSeasonDivisionAsync(seasonId, divisionId);
            if (sd == null) return;

            FloorballSeasonDivisionTeam? link = await _db.Set<FloorballSeasonDivisionTeam>()
                .FirstOrDefaultAsync(x => x.SeasonDivisionId == sd.Id && x.TeamId == teamId);
            if (link == null) return;

            _db.Set<FloorballSeasonDivisionTeam>().Remove(link);
            await _db.SaveChangesAsync();
        }
    }
}


