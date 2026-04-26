namespace user_service.DTOs.Auth;

public sealed class RegisterRequest
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Phone { get; set; }
    public long? TelegramChatId { get; set; }
}
