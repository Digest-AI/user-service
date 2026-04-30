namespace user_service.DTOs.Auth;

/// <summary>
/// Response sent after requesting a verification code for email verification (standalone).
/// Contains the purpose of the verification code.
/// </summary>
public sealed class SendVerificationCodeResponse
{
    public required string Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
}
