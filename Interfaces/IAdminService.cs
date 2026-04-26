using user_service.DTOs.Admin;
using user_service.DTOs.User;

namespace user_service.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyCollection<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<AdminUserDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> UpdateUserAsync(Guid id, AdminUpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> BlockUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UnblockUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserStatusAsync(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Models.Role>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> AddUserRolesAsync(Guid userId, AddUserRolesRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetUserRolesAsync(Guid userId, SetUserRolesRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    Task<NotificationSettingsDto?> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationSettingsDto?> UpdateUserNotificationsAsync(Guid userId, UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default);

    Task<long?> GetUserTelegramAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserTelegramAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Models.UserAction>> GetUserActionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<DashboardActivityDto> GetDashboardActivityAsync(CancellationToken cancellationToken = default);

    Task<int> BulkBlockAsync(BulkBlockRequest request, CancellationToken cancellationToken = default);
}
