using Domain.Entities.Floorball.Tournament;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball;

/// <summary>
/// Implementation for managing floorball tournament groups and their team memberships
/// </summary>
public class FloorballTournamentGroupRepository : IFloorballTournamentGroupRepository
{
    private readonly FloorballDbContext _db;

    public FloorballTournamentGroupRepository(FloorballDbContext db)
    {
        _db = db;
    }

    public async Task<FloorballTournamentGroup?> GetByIdAsync(Guid groupId)
    {
        return await _db.FloorballTournamentGroups
            .Include(g => g.Teams)
                .ThenInclude(gt => gt.Team)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<IEnumerable<FloorballTournamentGroup>> GetByTournamentIdAsync(Guid tournamentId)
    {
        return await _db.FloorballTournamentGroups
            .Include(g => g.Teams)
                .ThenInclude(gt => gt.Team)
            .Where(g => g.TournamentId == tournamentId)
            .OrderBy(g => g.Phase)
            .ThenBy(g => g.SortOrder)
            .ToListAsync();
    }

    public async Task AddAsync(FloorballTournamentGroup group)
    {
        await _db.FloorballTournamentGroups.AddAsync(group);
    }

    public async Task UpdateAsync(FloorballTournamentGroup group)
    {
        _db.Entry(group).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid groupId)
    {
        FloorballTournamentGroup? group = await _db.FloorballTournamentGroups.FindAsync(groupId);
        if (group != null)
        {
            _db.FloorballTournamentGroups.Remove(group);
        }
    }

    public async Task<bool> ExistsAsync(Guid groupId)
    {
        return await _db.FloorballTournamentGroups.AnyAsync(g => g.Id == groupId);
    }

    public async Task<FloorballTournamentGroupTeam> AddTeamToGroupAsync(Guid groupId, Guid teamId, Guid tournamentId)
    {
        bool exists = await _db.FloorballTournamentGroupTeams
            .AnyAsync(gt => gt.GroupId == groupId && gt.TeamId == teamId);
        if (exists)
            throw new InvalidOperationException("Team is already in this group.");

        FloorballTournamentGroupTeam membership = new(groupId, teamId, tournamentId);
        await _db.FloorballTournamentGroupTeams.AddAsync(membership);
        return membership;
    }

    public async Task RemoveTeamFromGroupAsync(Guid groupId, Guid teamId)
    {
        FloorballTournamentGroupTeam? membership = await _db.FloorballTournamentGroupTeams
            .FirstOrDefaultAsync(gt => gt.GroupId == groupId && gt.TeamId == teamId);
        if (membership == null)
            throw new ArgumentException($"Team with ID {teamId} is not in group {groupId}.");

        _db.FloorballTournamentGroupTeams.Remove(membership);
    }

    public async Task<IEnumerable<FloorballTournamentGroupTeam>> GetTeamsByGroupIdAsync(Guid groupId)
    {
        return await _db.FloorballTournamentGroupTeams
            .Include(gt => gt.Team)
            .Where(gt => gt.GroupId == groupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<FloorballTournamentGroup>> GetGroupsByTeamIdAsync(Guid tournamentId, Guid teamId)
    {
        HashSet<Guid> groupIds = await _db.FloorballTournamentGroupTeams
            .Where(gt => gt.TournamentId == tournamentId && gt.TeamId == teamId)
            .Select(gt => gt.GroupId)
            .ToHashSetAsync();

        return await _db.FloorballTournamentGroups
            .Include(g => g.Teams)
                .ThenInclude(gt => gt.Team)
            .Where(g => groupIds.Contains(g.Id))
            .OrderBy(g => g.Phase)
            .ThenBy(g => g.SortOrder)
            .ToListAsync();
    }
}
