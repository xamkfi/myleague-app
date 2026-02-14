using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Repository implementation for managing refresh tokens
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CommonDbContext _context;
    private readonly DbSet<RefreshToken> _entities;

    public RefreshTokenRepository(CommonDbContext context)
    {
        _context = context;
        _entities = context.Set<RefreshToken>();
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _entities
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
    {
        return await _entities
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _entities.AddAsync(refreshToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken)
    {
        _entities.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        List<RefreshToken> activeTokens = await _entities
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (RefreshToken token in activeTokens)
        {
            token.Revoke();
        }
    }
}
