namespace user_service.DTOs.Auth;

public sealed class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
