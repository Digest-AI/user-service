using user_service.DTOs.User;

namespace user_service.DTOs.Admin;

public sealed class AdminUserDto
{
    public Guid Id { get; set; }
    public Guid PublicId { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public sealed class AdminUpdateUserRequest
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public sealed class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class SetUserRolesRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}

public sealed class AddUserRolesRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}
