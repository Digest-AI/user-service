using user_service.DTOs.User;

namespace user_service.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> UpdateMeAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<bool> RequestEmailChangeAsync(Guid userId, RequestEmailChangeRequest request, CancellationToken cancellationToken = default);
    Task<bool> ConfirmEmailChangeAsync(Guid userId, ConfirmEmailChangeRequest request, CancellationToken cancellationToken = default);
    Task<bool> RequestPasswordChangeAsync(Guid userId, RequestPasswordChangeRequest request, CancellationToken cancellationToken = default);
    Task<bool> ConfirmPasswordChangeAsync(Guid userId, ConfirmPasswordChangeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
}
