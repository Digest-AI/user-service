using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.DTOs.Admin;
using user_service.DTOs.User;
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

    [HttpGet("users/{id:guid}/notifications")]
    public async Task<IActionResult> GetUserNotifications(Guid id, CancellationToken cancellationToken)
    {
        var result = await adminService.GetUserNotificationsAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("users/{id:guid}/notifications")]
    public async Task<IActionResult> UpdateUserNotifications(Guid id, [FromBody] UpdateNotificationSettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.UpdateUserNotificationsAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("users/{id:guid}/telegram")]
    public async Task<IActionResult> GetUserTelegram(Guid id, CancellationToken cancellationToken)
    {
        var result = await adminService.GetUserTelegramAsync(id, cancellationToken);
        return Ok(new { telegramChatId = result });
    }

    [HttpDelete("users/{id:guid}/telegram")]
    public async Task<IActionResult> DeleteUserTelegram(Guid id, CancellationToken cancellationToken)
    {
        var ok = await adminService.DeleteUserTelegramAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("users/{id:guid}/actions")]
    public async Task<IActionResult> GetUserActions(Guid id, CancellationToken cancellationToken)
        => Ok(await adminService.GetUserActionsAsync(id, cancellationToken));

    [HttpGet("users/{id:guid}/sessions")]
    public async Task<IActionResult> GetUserSessions(Guid id, CancellationToken cancellationToken)
        => Ok(await adminService.GetUserSessionsAsync(id, cancellationToken));

    [HttpDelete("users/{id:guid}/sessions")]
    public async Task<IActionResult> DeleteUserSessions(Guid id, CancellationToken cancellationToken)
    {
        var ok = await adminService.DeleteUserSessionsAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
        => Ok(await adminService.GetDashboardStatsAsync(cancellationToken));

    [HttpGet("dashboard/activity")]
    public async Task<IActionResult> GetDashboardActivity(CancellationToken cancellationToken)
        => Ok(await adminService.GetDashboardActivityAsync(cancellationToken));

    [HttpPost("users/bulk-block")]
    public async Task<IActionResult> BulkBlock([FromBody] BulkBlockRequest request, CancellationToken cancellationToken)
    {
        var affected = await adminService.BulkBlockAsync(request, cancellationToken);
        return Ok(new { affected });
    }

    [HttpPost("users/bulk-notify")]
    public IActionResult BulkNotify([FromBody] BulkNotifyRequest request)
    {
        return Ok(new { request.UserIds.Count, request.Message, sent = false, note = "Stub endpoint for integration with notification-service." });
    }
}
