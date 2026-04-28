namespace user_service.DTOs.Auth;

public sealed class RegistrationResponse
{
    public required string Email { get; set; }
    public DateTime VerificationCodeExpiresAt { get; set; }
    public string Message { get; set; } = "Verification code sent.";
}
