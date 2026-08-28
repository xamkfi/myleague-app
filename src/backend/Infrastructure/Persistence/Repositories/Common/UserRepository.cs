using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common;

/// <summary>
/// Repository implementation for managing users
/// </summary>
public class UserRepository : RepositoryBase<User, CommonDbContext>, IUserRepository
{
    public UserRepository(CommonDbContext context) : base(context)
    {
    }

    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _entities
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _entities
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPersonIdAsync(Guid personId)
    {
        return await _entities
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.PersonId == personId);
    }

    public override async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _entities
            .Include(u => u.Person)
            .ToListAsync();
    }

    public override async Task AddAsync(User user)
    {
        await _entities.AddAsync(user);
    }

    public override async Task UpdateAsync(User user)
    {
        _entities.Update(user);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        User? user = await _entities.FindAsync(id);
        if (user != null)
        {
            _entities.Remove(user);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _entities.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _entities.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByPersonIdAsync(Guid personId)
    {
        return await _entities.AnyAsync(u => u.PersonId == personId);
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string token)
    {
        return await _entities
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
    }

    public async Task<int> CountByRoleAsync(UserRole role)
    {
        return await _entities.CountAsync(u => u.Role == role);
    }
}
