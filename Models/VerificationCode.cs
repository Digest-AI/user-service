namespace user_service.Models;

public sealed class VerificationCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Code { get; set; }
    public VerificationCodePurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}
