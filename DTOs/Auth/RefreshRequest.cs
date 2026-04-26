namespace user_service.DTOs.Auth;

public sealed class RefreshRequest
{
    public required string RefreshToken { get; set; }
}
