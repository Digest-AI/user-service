using user_service.DTOs.Admin;
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

    private static AdminUserDto MapAdminUser(User user)
    {
        return new AdminUserDto
        {
            Id = user.Id,
            PublicId = user.PublicId,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToArray(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
