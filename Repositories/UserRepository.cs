using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Repositories;

public sealed class UserRepository(UserServiceDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetUserWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Include(x => x.Preferences)
            .Include(x => x.NotificationSettings)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Preference?> GetPreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Preferences.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<NotificationSettings?> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.NotificationSettings.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefreshToken>> GetSessionsAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.RefreshTokens.Where(x => x.UserId == userId);
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetSessionAsync(Guid userId, Guid sessionId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.RefreshTokens.Where(x => x.UserId == userId && x.Id == sessionId);
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<int> GetSessionsCountAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.RefreshTokens.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles.SingleOrDefaultAsync(x => x.Id == roleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserRole>> GetUserRoleLinksAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.UserRoles.Where(x => x.UserId == userId);
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetUserRoleLinkAsync(Guid userId, Guid roleId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.UserRoles.Where(x => x.UserId == userId && x.RoleId == roleId);
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserAction>> GetUserActionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserActions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserAction>> GetRecentActionsAsync(int take, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserActions
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
