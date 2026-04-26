using user_service.DTOs.Admin;
using user_service.DTOs.User;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Services;

public sealed class AdminService(IUserRepository userRepository) : IAdminService
{
    public async Task<IReadOnlyCollection<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetUsersAsync(cancellationToken);
        return users.Select(MapAdminUser).ToList();
    }

    public async Task<AdminUserDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(id, cancellationToken);
        return user is null ? null : MapAdminUser(user);
    }

    public async Task<AdminUserDto?> UpdateUserAsync(Guid id, AdminUpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Username)) user.Username = request.Username.Trim();
        if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName.Trim();
        user.Phone = request.Phone?.Trim() ?? user.Phone;
        if (request.IsVerified.HasValue) user.IsVerified = request.IsVerified.Value;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);
        return MapAdminUser(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.IsActive = false;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.RefreshTokensRevokedAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> BlockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await UpdateUserStatusAsync(id, new UpdateUserStatusRequest { IsActive = false }, cancellationToken);
    }

    public async Task<bool> UnblockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await UpdateUserStatusAsync(id, new UpdateUserStatusRequest { IsActive = true }, cancellationToken);
    }

    public async Task<bool> UpdateUserStatusAsync(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.IsActive = request.IsActive;
        if (request.IsVerified.HasValue)
        {
            user.IsVerified = request.IsVerified.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;
        if (!user.IsActive)
        {
            user.RefreshTokensRevokedAt = DateTime.UtcNow;
        }

        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return userRepository.GetRolesAsync(cancellationToken);
    }

    public async Task<bool> AddUserRolesAsync(Guid userId, AddUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        foreach (var roleId in request.RoleIds.Distinct())
        {
            var existing = await userRepository.GetUserRoleLinkAsync(userId, roleId, includeDeleted: true, cancellationToken);
            if (existing is not null)
            {
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                continue;
            }

            var role = await userRepository.GetRoleByIdAsync(roleId, cancellationToken);
            if (role is null)
            {
                continue;
            }

            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, IsDeleted = false });
        }

        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetUserRolesAsync(Guid userId, SetUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var allLinks = await userRepository.GetUserRoleLinksAsync(userId, includeDeleted: true, cancellationToken);
        var roleIds = request.RoleIds.Distinct().ToHashSet();

        foreach (var link in allLinks)
        {
            if (roleIds.Contains(link.RoleId))
            {
                link.IsDeleted = false;
                link.DeletedAt = null;
            }
            else if (!link.IsDeleted)
            {
                link.IsDeleted = true;
                link.DeletedAt = DateTime.UtcNow;
            }
        }

        var existingIds = allLinks.Select(x => x.RoleId).ToHashSet();
        foreach (var roleId in roleIds.Where(x => !existingIds.Contains(x)))
        {
            var role = await userRepository.GetRoleByIdAsync(roleId, cancellationToken);
            if (role is null)
            {
                continue;
            }

            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, IsDeleted = false });
        }

        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var relation = await userRepository.GetUserRoleLinkAsync(userId, roleId, includeDeleted: true, cancellationToken);
        if (relation is null || relation.IsDeleted)
        {
            return false;
        }

        relation.IsDeleted = true;
        relation.DeletedAt = DateTime.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<NotificationSettingsDto?> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = await userRepository.GetNotificationSettingsAsync(userId, cancellationToken);
        if (settings is null)
        {
            return null;
        }

        return new NotificationSettingsDto
        {
            EmailEnabled = settings.EmailEnabled,
            TelegramEnabled = settings.TelegramEnabled,
            PushEnabled = settings.PushEnabled,
            QuietHoursStart = settings.QuietHoursStart,
            QuietHoursEnd = settings.QuietHoursEnd
        };
    }

    public async Task<NotificationSettingsDto?> UpdateUserNotificationsAsync(Guid userId, UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await userRepository.GetNotificationSettingsAsync(userId, cancellationToken);
        if (settings is null)
        {
            return null;
        }

        settings.EmailEnabled = request.EmailEnabled;
        settings.TelegramEnabled = request.TelegramEnabled;
        settings.PushEnabled = request.PushEnabled;
        settings.QuietHoursStart = request.QuietHoursStart;
        settings.QuietHoursEnd = request.QuietHoursEnd;

        await userRepository.SaveChangesAsync(cancellationToken);

        return new NotificationSettingsDto
        {
            EmailEnabled = settings.EmailEnabled,
            TelegramEnabled = settings.TelegramEnabled,
            PushEnabled = settings.PushEnabled,
            QuietHoursStart = settings.QuietHoursStart,
            QuietHoursEnd = settings.QuietHoursEnd
        };
    }

    public async Task<long?> GetUserTelegramAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        return user?.TelegramChatId;
    }

    public async Task<bool> DeleteUserTelegramAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.TelegramChatId = null;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<IReadOnlyCollection<UserAction>> GetUserActionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return userRepository.GetUserActionsAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await userRepository.GetSessionsAsync(userId, cancellationToken);
        return sessions
            .Select(x => new UserSessionDto
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                ExpiresAt = x.ExpiresAt,
                Revoked = x.Revoked,
                RememberMe = x.RememberMe
            })
            .ToList();
    }

    public async Task<bool> DeleteUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await userRepository.GetSessionsAsync(userId, includeDeleted: true, cancellationToken);
        if (sessions.Count == 0)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        foreach (var session in sessions.Where(x => !x.IsDeleted))
        {
            session.Revoked = true;
            session.IsDeleted = true;
            session.DeletedAt = now;
        }

        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is not null)
        {
            user.RefreshTokensRevokedAt = now;
            user.UpdatedAt = now;
        }

        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetUsersAsync(cancellationToken);
        var sessions = await userRepository.GetSessionsCountAsync(cancellationToken);

        return new DashboardStatsDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(x => x.IsActive),
            VerifiedUsers = users.Count(x => x.IsVerified),
            SessionsCount = sessions
        };
    }

    public async Task<DashboardActivityDto> GetDashboardActivityAsync(CancellationToken cancellationToken = default)
    {
        var actions = await userRepository.GetRecentActionsAsync(50, cancellationToken);

        return new DashboardActivityDto
        {
            RecentActions = actions.Select(a => new UserActionActivityDto
            {
                UserId = a.UserId,
                EventId = a.EventId,
                ActionType = a.ActionType.ToString(),
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    public async Task<int> BulkBlockAsync(BulkBlockRequest request, CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetUsersAsync(cancellationToken);
        var ids = request.UserIds.ToHashSet();

        var affected = 0;
        var now = DateTime.UtcNow;
        foreach (var user in users.Where(x => ids.Contains(x.Id)))
        {
            user.IsActive = false;
            user.IsDeleted = true;
            user.DeletedAt = now;
            user.RefreshTokensRevokedAt = now;
            user.UpdatedAt = now;
            affected++;
        }

        if (affected > 0)
        {
            await userRepository.SaveChangesAsync(cancellationToken);
        }

        return affected;
    }

    private static AdminUserDto MapAdminUser(User user)
    {
        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToArray(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
