using Swashbuckle.AspNetCore.Filters;
using user_service.DTOs.Admin;

namespace user_service.Swagger.Examples.Admin;

public class AdminUserDtoExample : IExamplesProvider<AdminUserDto>
{
    public AdminUserDto GetExamples()
    {
        return new AdminUserDto
        {
            Id = Guid.NewGuid(),
            PublicId = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin",
            Surname = "User",
            Age = 35,
            Gender = "Male",
            IsVerified = true,
            DateJoined = DateTime.UtcNow.AddYears(-2),
            DateDeleted = null,
            IsDeleted = false,
            Roles = new List<string> { "ADMIN", "USER" }
        };
    }
}

public class AdminCreateUserRequestExample : IExamplesProvider<AdminCreateUserRequest>
{
    public AdminCreateUserRequest GetExamples()
    {
        return new AdminCreateUserRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            Name = "John",
            Surname = "Doe",
            Age = 30,
            Gender = "Male",
            IsVerified = true,
            DateJoined = DateTime.UtcNow,
            DateDeleted = null,
            IsDeleted = false
        };
    }
}

public class AdminUpdateUserRequestExample : IExamplesProvider<AdminUpdateUserRequest>
{
    public AdminUpdateUserRequest GetExamples()
    {
        return new AdminUpdateUserRequest
        {
            Email = "updatedemail@example.com",
            Name = "Jane",
            Surname = "Smith",
            Age = 28,
            Gender = "Female",
            IsVerified = true
        };
    }
}

public class SetUserRolesRequestExample : IExamplesProvider<SetUserRolesRequest>
{
    public SetUserRolesRequest GetExamples()
    {
        return new SetUserRolesRequest
        {
            RoleIds = new List<Guid>
            {
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002")
            }
        };
    }
}

public class AddUserRolesRequestExample : IExamplesProvider<AddUserRolesRequest>
{
    public AddUserRolesRequest GetExamples()
    {
        return new AddUserRolesRequest
        {
            RoleIds = new List<Guid>
            {
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            }
        };
    }
}
