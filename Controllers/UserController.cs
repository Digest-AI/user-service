using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.DTOs.User;
using user_service.Interfaces;

namespace user_service.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public sealed class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var profile = await userService.GetMeAsync(userId.Value, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var profile = await userService.UpdateMeAsync(userId.Value, request, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var success = await userService.ChangePasswordAsync(userId.Value, request, cancellationToken);
        return success ? NoContent() : BadRequest(new { message = "Invalid current password or user not found." });
    }

    [HttpDelete("deactivate")]
    public async Task<IActionResult> Deactivate(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var success = await userService.DeactivateAsync(userId.Value, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await userService.GetPreferencesAsync(userId.Value, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferenceRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await userService.UpdatePreferencesAsync(userId.Value, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("notifications/settings")]
    public async Task<IActionResult> GetNotificationSettings(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await userService.GetNotificationSettingsAsync(userId.Value, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("notifications/settings")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await userService.UpdateNotificationSettingsAsync(userId.Value, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("telegram/connect")]
    public async Task<IActionResult> ConnectTelegram([FromBody] ConnectTelegramRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var success = await userService.ConnectTelegramAsync(userId.Value, request.TelegramChatId, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("telegram/disconnect")]
    public async Task<IActionResult> DisconnectTelegram(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var success = await userService.DisconnectTelegramAsync(userId.Value, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var sessions = await userService.GetSessionsAsync(userId.Value, cancellationToken);
        return Ok(sessions);
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var success = await userService.DeleteSessionAsync(userId.Value, sessionId, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("sessions/all")]
    public async Task<IActionResult> DeleteAllSessions(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var success = await userService.DeleteAllSessionsAsync(userId.Value, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var value) ? value : null;
    }
}
