namespace user_service.Models;

public sealed class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Phone { get; set; }
    public long? TelegramChatId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public DateTime? RefreshTokensRevokedAt { get; set; }

    public Preference? Preferences { get; set; }
    public NotificationSettings? NotificationSettings { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserAction> UserActions { get; set; } = new List<UserAction>();
}
