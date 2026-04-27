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
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken) => Ok(await adminService.GetUsersAsync(cancellationToken));

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await adminService.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await adminService.UpdateUserAsync(id, request, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var ok = await adminService.DeleteUserAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("users/{id:guid}/block")]
    public async Task<IActionResult> BlockUser(Guid id, CancellationToken cancellationToken)
    {
        var ok = await adminService.BlockUserAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("users/{id:guid}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid id, CancellationToken cancellationToken)
    {
        var ok = await adminService.UnblockUserAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var ok = await adminService.UpdateUserStatusAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken) => Ok(await adminService.GetRolesAsync(cancellationToken));

    [HttpPost("users/{id:guid}/roles")]
    public async Task<IActionResult> AddRoles(Guid id, [FromBody] AddUserRolesRequest request, CancellationToken cancellationToken)
    {
        var ok = await adminService.AddUserRolesAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPut("users/{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var ok = await adminService.SetUserRolesAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("users/{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id, Guid roleId, CancellationToken cancellationToken)
    {
        var ok = await adminService.DeleteUserRoleAsync(id, roleId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
