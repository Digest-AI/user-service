namespace user_service.Models;

public sealed class User
{
    public Guid Id { get; set; }
    public Guid PublicId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsVerified { get; set; }
    public DateTime DateJoined { get; set; } = DateTime.UtcNow;
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
    public DateTime? DateDeleted { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
    public ICollection<Chain> Chains { get; set; } = new List<Chain>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
