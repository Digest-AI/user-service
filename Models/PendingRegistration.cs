namespace user_service.Models;

public sealed class PendingRegistration
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
    public required string Code { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
