using user_service.Models;

namespace user_service.Interfaces;

public interface IJwtTokenService
{
    (string AccessToken, DateTime ExpiresAt) CreateAccessToken(User user, IReadOnlyCollection<string> roles);
    (string RawToken, string TokenHash, DateTime ExpiresAt) CreateRefreshToken(bool rememberMe);
    string ComputeRefreshTokenHash(string rawToken);
}
