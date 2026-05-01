using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Repositories;

public sealed class UserRepository(UserServiceDbContext dbContext) : IUserRepository
{
    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .SingleOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Where(x => !x.IsDeleted)
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .OrderByDescending(x => x.DateJoined)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users
            .Where(x => x.Email == email && !x.IsDeleted);

        if (excludeUserId.HasValue)
        {
            query = query.Where(x => x.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefreshToken>> GetSessionsAsync(
        Guid userId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.RefreshTokens
            .Where(x => x.UserId == userId);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetSessionAsync(
        Guid userId,
        Guid sessionId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.RefreshTokens
            .Where(x => x.UserId == userId && x.Id == sessionId);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .SingleOrDefaultAsync(x => x.Id == roleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserRole>> GetUserRoleLinksAsync(
        Guid userId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.UserRoles
            .Where(x => x.UserId == userId);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetUserRoleLinkAsync(
        Guid userId,
        Guid roleId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.UserRoles
            .Where(x => x.UserId == userId && x.RoleId == roleId);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}