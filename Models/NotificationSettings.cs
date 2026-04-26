namespace user_service.Models;

public sealed class NotificationSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public bool EmailEnabled { get; set; } = true;
    public bool TelegramEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;

    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }

    public User User { get; set; } = null!;
}
