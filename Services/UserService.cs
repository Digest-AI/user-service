using BCrypt.Net;
using user_service.DTOs.User;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Services;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserProfileDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(userId, cancellationToken);
        return user is null ? null : MapProfile(user);
    }

    public async Task<UserProfileDto?> UpdateMeAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.Username = request.Username.Trim();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);
        return MapProfile(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        user.RefreshTokensRevokedAt = DateTime.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
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

    private static UserProfileDto MapProfile(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            PublicId = user.PublicId,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToArray()
        };
    }
}
