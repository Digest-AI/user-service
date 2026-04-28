namespace user_service.DTOs.Auth;

public sealed class AuthResponse
{
    public required Guid PublicId { get; set; }
    public required Guid ChainId { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}
