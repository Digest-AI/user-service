using user_service.DTOs.User;
using user_service.Models;

namespace user_service.Interfaces;

public interface IInternalUserService
{
    Task<PreferenceDto?> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserAction>> GetActionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationSettingsDto?> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<long?> GetTelegramAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    bool ValidateToken(string token);
}
