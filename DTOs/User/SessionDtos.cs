namespace user_service.DTOs.User;

public sealed class UserSessionDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public bool RememberMe { get; set; }
}

public sealed class ConnectTelegramRequest
{
    public long TelegramChatId { get; set; }
}
