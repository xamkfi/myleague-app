using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Hockey;

/// <summary>
/// EF Core repository for hockey players.
/// </summary>
public class HockeyPlayerRepository : IHockeyPlayerRepository
{
    private readonly HockeyDbContext _dbContext;

    public HockeyPlayerRepository(HockeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HockeyPlayer player)
    {
        await _dbContext.HockeyPlayers.AddAsync(player);
    }

    public async Task<HockeyPlayer?> GetByIdAsync(Guid id)
    {
        return await _dbContext.HockeyPlayers
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<HockeyPlayer?> GetByPersonIdAsync(Guid personId)
    {
        return await _dbContext.HockeyPlayers
            .FirstOrDefaultAsync(p => p.PersonId == personId);
    }

    public async Task<bool> HasCompetitionHistoryAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        HockeyPlayer? player = await _dbContext.HockeyPlayers
            .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
        if (player != null && player.CareerGamesPlayed > 0)
        {
            return true;
        }

        bool hasRosterGames = await _dbContext.HockeyTeamPlayers
            .AnyAsync(tp => tp.PlayerId == playerId && tp.GamesPlayed > 0, cancellationToken);
        if (hasRosterGames)
        {
            return true;
        }

        bool hasStats = await _dbContext.HockeyPlayerCompetitionStatistics
                .AnyAsync(s => s.PlayerId == playerId, cancellationToken)
            || await _dbContext.HockeyMatchPlayerStatistics
                .AnyAsync(s => s.PlayerId == playerId, cancellationToken);
        if (hasStats)
        {
            return true;
        }

        return await (
            from teamPlayer in _dbContext.HockeyTeamPlayers
            join activePlayer in _dbContext.HockeyMatchActivePlayers
                on teamPlayer.Id equals activePlayer.TeamPlayerId
            join selection in _dbContext.HockeyMatchPlayerSelections
                on activePlayer.MatchPlayerSelectionId equals selection.Id
            join matchTeam in _dbContext.HockeyMatchTeams
                on selection.MatchTeamId equals matchTeam.Id
            join match in _dbContext.HockeyMatches
                on matchTeam.MatchId equals match.Id
            where teamPlayer.PlayerId == playerId
                && match.Status != Domain.Enums.Hockey.Matches.HockeyMatchStatus.Scheduled
            select teamPlayer.Id
        ).AnyAsync(cancellationToken);
    }

    public async Task DeleteUnusedProfileAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        List<HockeyTeamPlayer> rosterRows = await _dbContext.HockeyTeamPlayers
            .Where(tp => tp.PlayerId == playerId)
            .ToListAsync(cancellationToken);
        _dbContext.HockeyTeamPlayers.RemoveRange(rosterRows);

        HockeyPlayer? player = await _dbContext.HockeyPlayers
            .FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
        if (player != null)
        {
            _dbContext.HockeyPlayers.Remove(player);
        }
    }
}
