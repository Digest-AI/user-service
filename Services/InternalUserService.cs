using System.IdentityModel.Tokens.Jwt;
using user_service.DTOs.User;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Services;

public sealed class InternalUserService(IUserRepository userRepository) : IInternalUserService
{
    public async Task<PreferenceDto?> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preference = await userRepository.GetPreferenceAsync(userId, cancellationToken);
        if (preference is null)
        {
            return null;
        }

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

    public Task<IReadOnlyCollection<UserAction>> GetActionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return userRepository.GetUserActionsAsync(userId, cancellationToken);
    }

    public async Task<NotificationSettingsDto?> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default)
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

    public async Task<long?> GetTelegramAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        return user?.TelegramChatId;
    }

    public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return userRepository.GetUserByIdAsync(userId, cancellationToken);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var handler = new JwtSecurityTokenHandler();
        return handler.CanReadToken(token);
    }
}
