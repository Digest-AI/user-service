using user_service.DTOs.User;

namespace user_service.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> UpdateMeAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PreferenceDto?> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PreferenceDto?> UpdatePreferencesAsync(Guid userId, UpdatePreferenceRequest request, CancellationToken cancellationToken = default);

    Task<NotificationSettingsDto?> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationSettingsDto?> UpdateNotificationSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default);

    Task<bool> ConnectTelegramAsync(Guid userId, long telegramChatId, CancellationToken cancellationToken = default);
    Task<bool> DisconnectTelegramAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserSessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
