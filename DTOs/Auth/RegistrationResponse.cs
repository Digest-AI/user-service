namespace user_service.DTOs.Auth;

public sealed class RegistrationResponse
{
<<<<<<< Updated upstream
    public required Guid PublicId { get; set; }
=======
    public Guid? PublicId { get; set; }
>>>>>>> Stashed changes
    public required string Email { get; set; }
    public DateTime VerificationCodeExpiresAt { get; set; }
    public required string Purpose { get; set; }
    public string Message { get; set; } = "Verification code sent.";
}
