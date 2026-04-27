using user_service.DTOs.Admin;
using user_service.Models;

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

    Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> AddUserRolesAsync(Guid userId, AddUserRolesRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetUserRolesAsync(Guid userId, SetUserRolesRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}
