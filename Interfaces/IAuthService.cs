using user_service.DTOs.Auth;

namespace user_service.Interfaces;

public interface IAuthService
{
    Task<RegistrationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<RegistrationResponse?> ResendRegistrationCodeAsync(ResendRegistrationCodeRequest request, CancellationToken cancellationToken = default);
    Task<AuthUserDto?> ConfirmRegistrationAsync(ConfirmRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAllDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
}
