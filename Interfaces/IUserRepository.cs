using user_service.Models;

namespace user_service.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RefreshToken>> GetSessionsAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetSessionAsync(Guid userId, Guid sessionId, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserRole>> GetUserRoleLinksAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<UserRole?> GetUserRoleLinkAsync(Guid userId, Guid roleId, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
