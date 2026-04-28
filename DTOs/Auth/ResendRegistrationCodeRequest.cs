namespace user_service.DTOs.Auth;

public sealed class ResendRegistrationCodeRequest
{
    public required string Email { get; set; }
}
