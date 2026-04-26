namespace user_service.DTOs.User;

public sealed class NotificationSettingsDto
{
    public bool EmailEnabled { get; set; }
    public bool TelegramEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
}

public sealed class UpdateNotificationSettingsRequest
{
    public bool EmailEnabled { get; set; }
    public bool TelegramEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
}
