namespace user_service.Models;

public sealed class Chain
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool RememberMe { get; set; }

    public User User { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
