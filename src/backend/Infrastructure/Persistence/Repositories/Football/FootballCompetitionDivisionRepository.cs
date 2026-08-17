using Domain.Entities.Football.Competitions;
using Domain.Repositories.Football;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Football
{
    /// <summary>
    /// Implementation for managing football competition divisions and memberships
    /// </summary>
    public class FootballCompetitionDivisionRepository : IFootballCompetitionDivisionRepository
    {
        private readonly FootballDbContext _db;

        public FootballCompetitionDivisionRepository(FootballDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FootballCompetitionDivision>> GetCompetitionDivisionsAsync(Guid competitionId)
        {
            return await _db.Set<FootballCompetitionDivision>()
                .Include(sd => sd.Teams)
                .Where(sd => sd.CompetitionId == competitionId)
                .ToListAsync();
        }

        public async Task<FootballCompetitionDivision?> GetCompetitionDivisionAsync(Guid competitionId, Guid divisionId)
        {
            return await _db.Set<FootballCompetitionDivision>()
                .Include(sd => sd.Teams)
                .FirstOrDefaultAsync(sd => sd.CompetitionId == competitionId && sd.DivisionId == divisionId);
        }

        public async Task<IEnumerable<FootballCompetition>> GetCompetitionsByDivisionAsync(Guid divisionId)
        {
            HashSet<Guid> competitionIds = await _db.Set<FootballCompetitionDivision>()
                .Where(sd => sd.DivisionId == divisionId)
                .Select(sd => sd.CompetitionId)
                .ToHashSetAsync();

            return await _db.FootballCompetitions
                .Include(s => s.Matches)
                .Where(s => competitionIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<FootballCompetition>> GetCompetitionsByTeamAsync(Guid teamId)
        {
            HashSet<Guid> competitionIds = await _db.Set<FootballCompetitionDivisionTeam>()
                .Where(sdt => sdt.TeamId == teamId)
                .Select(sdt => sdt.CompetitionId)
                .ToHashSetAsync();

            return await _db.FootballCompetitions
                .Include(s => s.Matches)
                .Where(s => competitionIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<FootballCompetitionDivisionTeam>> GetCompetitionDivisionTeamsAsync(Guid competitionId)
        {
            return await _db.Set<FootballCompetitionDivisionTeam>()
                .Include(sdt => sdt.Team)
                .Where(sdt => sdt.CompetitionId == competitionId)
                .ToListAsync();
        }

        public async Task AddCompetitionDivisionAsync(Guid competitionId, Guid divisionId)
        {
            bool exists = await _db.Set<FootballCompetitionDivision>().AnyAsync(sd => sd.CompetitionId == competitionId && sd.DivisionId == divisionId);
            if (exists) return;

            _db.Set<FootballCompetitionDivision>().Add(new FootballCompetitionDivision(competitionId, divisionId));
            await _db.SaveChangesAsync();
        }

        public async Task RemoveCompetitionDivisionAsync(Guid competitionId, Guid divisionId)
        {
            FootballCompetitionDivision? sd = await _db.Set<FootballCompetitionDivision>()
                .FirstOrDefaultAsync(x => x.CompetitionId == competitionId && x.DivisionId == divisionId);
            if (sd == null) return;

            _db.Set<FootballCompetitionDivision>().Remove(sd);
            await _db.SaveChangesAsync();
        }

        public async Task AddTeamToCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId)
        {
            FootballCompetitionDivision? sd = await GetCompetitionDivisionAsync(competitionId, divisionId);
            if (sd == null)
            {
                sd = new FootballCompetitionDivision(competitionId, divisionId);
                _db.Set<FootballCompetitionDivision>().Add(sd);
                await _db.SaveChangesAsync();
            }

            bool exists = await _db.Set<FootballCompetitionDivisionTeam>()
                .AnyAsync(x => x.CompetitionDivisionId == sd.Id && x.TeamId == teamId);
            if (exists) return;

            _db.Set<FootballCompetitionDivisionTeam>().Add(new FootballCompetitionDivisionTeam(sd.Id, teamId, competitionId));
            await _db.SaveChangesAsync();
        }

        public async Task RemoveTeamFromCompetitionDivisionAsync(Guid competitionId, Guid divisionId, Guid teamId)
        {
            FootballCompetitionDivision? sd = await GetCompetitionDivisionAsync(competitionId, divisionId);
            if (sd == null) return;

            FootballCompetitionDivisionTeam? link = await _db.Set<FootballCompetitionDivisionTeam>()
                .FirstOrDefaultAsync(x => x.CompetitionDivisionId == sd.Id && x.TeamId == teamId);
            if (link == null) return;

            _db.Set<FootballCompetitionDivisionTeam>().Remove(link);
            await _db.SaveChangesAsync();
        }
    }
}
