using user_service.DTOs.User;

namespace user_service.DTOs.Admin;

public sealed class AdminUserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public sealed class AdminUpdateUserRequest
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public bool? IsVerified { get; set; }
}

public sealed class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
    public bool? IsVerified { get; set; }
}

public sealed class SetUserRolesRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}

public sealed class AddUserRolesRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}

public sealed class BulkBlockRequest
{
    public List<Guid> UserIds { get; set; } = [];
}

public sealed class BulkNotifyRequest
{
    public List<Guid> UserIds { get; set; } = [];
    public required string Message { get; set; }
}

public sealed class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int SessionsCount { get; set; }
}

public sealed class DashboardActivityDto
{
    public IReadOnlyCollection<UserActionActivityDto> RecentActions { get; set; } = [];
}

public sealed class UserActionActivityDto
{
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
