namespace user_service.DTOs.User;

public sealed class UserProfileDto
{
    public Guid PublicId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
    public bool IsVerified { get; set; }
    public DateTime DateJoined { get; set; }
    public DateTime? DateDeleted { get; set; }
    public bool IsDeleted { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = [];
}

public sealed class UpdateProfileRequest
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
}

public sealed class ChangePasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
