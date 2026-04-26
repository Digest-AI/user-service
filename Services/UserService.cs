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
        user.Phone = request.Phone?.Trim();
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

    public async Task<PreferenceDto?> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preference = await userRepository.GetPreferenceAsync(userId, cancellationToken);
        return preference is null ? null : MapPreference(preference);
    }

    public async Task<PreferenceDto?> UpdatePreferencesAsync(Guid userId, UpdatePreferenceRequest request, CancellationToken cancellationToken = default)
    {
        var preference = await userRepository.GetPreferenceAsync(userId, cancellationToken);
        if (preference is null)
        {
            return null;
        }

        preference.FavoriteCategories = request.FavoriteCategories;
        preference.FavoriteArtists = request.FavoriteArtists;
        preference.PreferredCities = request.PreferredCities;
        preference.MinPrice = request.MinPrice;
        preference.MaxPrice = request.MaxPrice;
        preference.PreferredEventTime = request.PreferredEventTime;
        preference.NotificationBeforeHours = request.NotificationBeforeHours;

        await userRepository.SaveChangesAsync(cancellationToken);
        return MapPreference(preference);
    }

    public async Task<NotificationSettingsDto?> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = await userRepository.GetNotificationSettingsAsync(userId, cancellationToken);
        return settings is null ? null : MapNotificationSettings(settings);
    }

    public async Task<NotificationSettingsDto?> UpdateNotificationSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default)
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
        return MapNotificationSettings(settings);
    }

    public async Task<bool> ConnectTelegramAsync(Guid userId, long telegramChatId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.TelegramChatId = telegramChatId;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DisconnectTelegramAsync(Guid userId, CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyCollection<UserSessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await userRepository.GetSessionsAsync(userId, cancellationToken);
        return sessions.Select(MapSession).ToList();
    }

    public async Task<bool> DeleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await userRepository.GetSessionAsync(userId, sessionId, includeDeleted: true, cancellationToken);
        if (session is null || session.IsDeleted)
        {
            return false;
        }

        session.Revoked = true;
        session.IsDeleted = true;
        session.DeletedAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var sessions = await userRepository.GetSessionsAsync(userId, includeDeleted: true, cancellationToken);
        foreach (var session in sessions.Where(x => !x.IsDeleted))
        {
            session.Revoked = true;
            session.IsDeleted = true;
            session.DeletedAt = now;
        }

        user.RefreshTokensRevokedAt = now;
        user.UpdatedAt = now;

        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserProfileDto MapProfile(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            TelegramChatId = user.TelegramChatId,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToArray()
        };
    }

    private static PreferenceDto MapPreference(Preference preference)
    {
        return new PreferenceDto
        {
            FavoriteCategories = preference.FavoriteCategories,
            FavoriteArtists = preference.FavoriteArtists,
            PreferredCities = preference.PreferredCities,
            MinPrice = preference.MinPrice,
            MaxPrice = preference.MaxPrice,
            PreferredEventTime = preference.PreferredEventTime,
            NotificationBeforeHours = preference.NotificationBeforeHours
        };
    }

    private static NotificationSettingsDto MapNotificationSettings(NotificationSettings settings)
    {
        return new NotificationSettingsDto
        {
            EmailEnabled = settings.EmailEnabled,
            TelegramEnabled = settings.TelegramEnabled,
            PushEnabled = settings.PushEnabled,
            QuietHoursStart = settings.QuietHoursStart,
            QuietHoursEnd = settings.QuietHoursEnd
        };
    }

    private static UserSessionDto MapSession(RefreshToken session)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            CreatedAt = session.CreatedAt,
            ExpiresAt = session.ExpiresAt,
            Revoked = session.Revoked,
            RememberMe = session.RememberMe
        };
    }
}
