using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using user_service.DTOs.Admin;
using user_service.Interfaces;
using user_service.Models;
using user_service.Validation;

namespace user_service.Services;

public sealed class AdminService(IUserRepository userRepository, IVerificationCodeService verificationCodeService) : IAdminService
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

    public async Task<AdminUserDto?> CreateUserAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
        UserInputValidation.ValidatePassword(request.Password);

        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("email_already_exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            PublicId = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsVerified = request.IsVerified,
            DateJoined = request.DateJoined ?? DateTime.UtcNow,
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Age = request.Age,
            Gender = request.Gender.Trim(),
            DateDeleted = request.DateDeleted,
            IsDeleted = request.IsDeleted
        };

        var defaultRole = await userRepository.GetRolesAsync(cancellationToken).ContinueWith(t => t.Result.Single(x => x.Name == "USER"), cancellationToken);
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id, IsDeleted = false });

        await userRepository.AddUserAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        await verificationCodeService.CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.EMAIL, cancellationToken);
        return MapAdminUser(user);
    }

    public async Task<AdminUserDto?> UpdateUserAsync(Guid id, AdminUpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
            if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                if (await userRepository.EmailExistsAsync(normalizedEmail, id, cancellationToken))
                {
                    throw new InvalidOperationException("email_already_exists");
                }

                user.Email = normalizedEmail;
                user.IsVerified = false;
                await verificationCodeService.CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.EMAIL, cancellationToken);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            UserInputValidation.ValidatePassword(request.Password);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) user.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.Surname)) user.Surname = request.Surname.Trim();
        if (request.Age.HasValue) user.Age = request.Age.Value;
        if (!string.IsNullOrWhiteSpace(request.Gender)) user.Gender = request.Gender.Trim();
        if (request.IsVerified.HasValue) user.IsVerified = request.IsVerified.Value;
        if (request.DateJoined.HasValue) user.DateJoined = request.DateJoined.Value;
        if (request.DateDeleted.HasValue) user.DateDeleted = request.DateDeleted.Value;
        if (request.IsDeleted.HasValue) user.IsDeleted = request.IsDeleted.Value;

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

        user.DateDeleted = DateTime.UtcNow;
        user.IsDeleted = true;
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
            Name = user.Name,
            Surname = user.Surname,
            Age = user.Age,
            Gender = user.Gender,
            IsVerified = user.IsVerified,
            DateJoined = user.DateJoined,
            DateDeleted = user.DateDeleted,
            IsDeleted = user.IsDeleted,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToArray()
        };
    }
}
