using user_service.DTOs.User;

namespace user_service.DTOs.Admin;

public sealed class AdminUserDto
{
    public Guid Id { get; set; }
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

public sealed class AdminCreateUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? DateJoined { get; set; }
    public DateTime? DateDeleted { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class AdminUpdateUserRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public bool? IsVerified { get; set; }
    public DateTime? DateJoined { get; set; }
    public DateTime? DateDeleted { get; set; }
    public bool? IsDeleted { get; set; }
}

public sealed class SetUserRolesRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}

public sealed class AddUserRolesRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}
