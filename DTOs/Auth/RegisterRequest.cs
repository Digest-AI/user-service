namespace user_service.DTOs.Auth;

public sealed class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
}
