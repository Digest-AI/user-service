using user_service.DTOs.Auth;

namespace user_service.Interfaces;

public interface IAuthService
{
    Task<VerificationInitiationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<VerificationInitiationResponse?> ResendRegistrationCodeAsync(ResendRegistrationCodeRequest request, CancellationToken cancellationToken = default);
    Task<AuthSuccessResponse?> ConfirmRegistrationAsync(ConfirmRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<VerificationCodeResponse> RestorePasswordAsync(RestorePasswordRequest request, CancellationToken cancellationToken = default);
    Task<VerificationCodeResponse?> ResendRestorePasswordCodeAsync(ResendRestorePasswordCodeRequest request, CancellationToken cancellationToken = default);
    Task<bool> ConfirmRestorePasswordAsync(ConfirmRestorePasswordRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthSuccessResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAllDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
}
