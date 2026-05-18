using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Implementation for managing floorball competition divisions and memberships
    /// </summary>
    public class FloorballCompetitionDivisionRepository : IFloorballCompetitionDivisionRepository
    {
        private readonly FloorballDbContext _db;

        public FloorballCompetitionDivisionRepository(FloorballDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FloorballCompetitionDivision>> GetCompetitionDivisionsAsync(Guid competitionId)
        {
            return await _db.Set<FloorballCompetitionDivision>()
                .Include(sd => sd.Teams)
                .Where(sd => sd.CompetitionId == competitionId)
                .ToListAsync();
        }

        public async Task<FloorballCompetitionDivision?> GetCompetitionDivisionAsync(Guid competitionId, Guid divisionId)
        {
            return await _db.Set<FloorballCompetitionDivision>()
                .Include(sd => sd.Teams)
                .FirstOrDefaultAsync(sd => sd.CompetitionId == competitionId && sd.DivisionId == divisionId);
        }

        public async Task<IEnumerable<FloorballCompetition>> GetCompetitionsByDivisionAsync(Guid divisionId)
        {
            HashSet<Guid> competitionIds = await _db.Set<FloorballCompetitionDivision>()
                .Where(sd => sd.DivisionId == divisionId)
                .Select(sd => sd.CompetitionId)
                .ToHashSetAsync();

            return await _db.FloorballCompetitions
                .Include(s => s.Matches)
                .Where(s => competitionIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<FloorballCompetition>> GetCompetitionsByTeamAsync(Guid teamId)
        {
            HashSet<Guid> competitionIds = await _db.Set<FloorballCompetitionDivisionTeam>()
                .Where(sdt => sdt.TeamId == teamId)
                .Select(sdt => sdt.CompetitionId)
                .ToHashSetAsync();

            return await _db.FloorballCompetitions
                .Include(s => s.Matches)
                .Where(s => competitionIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<FloorballCompetitionDivisionTeam>> GetCompetitionDivisionTeamsAsync(Guid competitionId)
        {
            return await _db.Set<FloorballCompetitionDivisionTeam>()
                .Include(sdt => sdt.Team)
                .Where(sdt => sdt.CompetitionId == competitionId)
                .ToListAsync();
        }

        public async Task AddCompetitionDivisionAsync(Guid competitionId, Guid divisionId)
        {
            bool exists = await _db.Set<FloorballCompetitionDivision>().AnyAsync(sd => sd.CompetitionId == competitionId && sd.DivisionId == divisionId);
            if (exists) return;

            _db.Set<FloorballCompetitionDivision>().Add(new FloorballCompetitionDivision(competitionId, divisionId));
            await _db.SaveChangesAsync();
        }

        public async Task RemoveCompetitionDivisionAsync(Guid competitionId, Guid divisionId)
        {
            FloorballCompetitionDivision? sd = await _db.Set<FloorballCompetitionDivision>()
                .FirstOrDefaultAsync(x => x.CompetitionId == competitionId && x.DivisionId == divisionId);
            if (sd == null) return;

            _db.Set<FloorballCompetitionDivision>().Remove(sd);
            await _db.SaveChangesAsync();
        }

        public async Task AddTeamToCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId)
        {
            FloorballCompetitionDivision? sd = await GetCompetitionDivisionAsync(competitionId, divisionId);
            if (sd == null)
            {
                sd = new FloorballCompetitionDivision(competitionId, divisionId);
                _db.Set<FloorballCompetitionDivision>().Add(sd);
                await _db.SaveChangesAsync();
            }

            bool exists = await _db.Set<FloorballCompetitionDivisionTeam>()
                .AnyAsync(x => x.CompetitionDivisionId == sd.Id && x.TeamId == teamId);
            if (exists) return;

            _db.Set<FloorballCompetitionDivisionTeam>().Add(new FloorballCompetitionDivisionTeam(sd.Id, teamId, competitionId));
            await _db.SaveChangesAsync();
        }

        public async Task RemoveTeamFromCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId)
        {
            FloorballCompetitionDivision? sd = await GetCompetitionDivisionAsync(competitionId, divisionId);
            if (sd == null) return;

            FloorballCompetitionDivisionTeam? link = await _db.Set<FloorballCompetitionDivisionTeam>()
                .FirstOrDefaultAsync(x => x.CompetitionDivisionId == sd.Id && x.TeamId == teamId);
            if (link == null) return;

            _db.Set<FloorballCompetitionDivisionTeam>().Remove(link);
            await _db.SaveChangesAsync();
        }
    }
}
