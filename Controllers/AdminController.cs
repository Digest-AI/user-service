using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.DTOs.Admin;
using user_service.Interfaces;

namespace user_service.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    /// <summary>Gets all users.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken) => Ok(await adminService.GetUsersAsync(cancellationToken));

    /// <summary>Gets a user by ID.</summary>
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await adminService.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Creates a user.</summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await adminService.CreateUserAsync(request, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Updates a user.</summary>
    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await adminService.UpdateUserAsync(id, request, cancellationToken);
            return user is null ? NotFound() : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Deletes a user.</summary>
    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var ok = await adminService.DeleteUserAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Gets roles.</summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken) => Ok(await adminService.GetRolesAsync(cancellationToken));

    /// <summary>Adds roles to a user.</summary>
    [HttpPost("users/{id:guid}/roles")]
    public async Task<IActionResult> AddRoles(Guid id, [FromBody] AddUserRolesRequest request, CancellationToken cancellationToken)
    {
        var ok = await adminService.AddUserRolesAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Replaces user roles.</summary>
    [HttpPut("users/{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var ok = await adminService.SetUserRolesAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Deletes a role link from a user.</summary>
    [HttpDelete("users/{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id, Guid roleId, CancellationToken cancellationToken)
    {
        var ok = await adminService.DeleteUserRoleAsync(id, roleId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
