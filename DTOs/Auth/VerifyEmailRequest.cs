namespace user_service.DTOs.Auth;

public sealed class VerifyEmailRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}
