using user_service.DTOs.User;

namespace user_service.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> UpdateMeAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
}
