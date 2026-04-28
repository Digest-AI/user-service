namespace user_service.DTOs.Auth;

public sealed class AuthUserDto
{
    public required Guid PublicId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
    public bool IsVerified { get; set; }
    public DateTime DateJoined { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = [];
}
