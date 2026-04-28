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
    /// <summary>Gets the current user's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var profile = await userService.GetMeAsync(userId.Value, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Updates the current user's age.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var profile = await userService.UpdateMeAsync(userId.Value, request, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Requests a verification code for changing email.</summary>
    [HttpPost("me/email/resend-code")]
    public async Task<IActionResult> ResendEmailCode([FromBody] RequestEmailChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var ok = await userService.RequestEmailChangeAsync(userId.Value, request, cancellationToken);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Confirms email change using a verification code.</summary>
    [HttpPost("me/email/confirm")]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var ok = await userService.ConfirmEmailChangeAsync(userId.Value, request, cancellationToken);
            return ok ? NoContent() : BadRequest(new { message = "Invalid or expired verification code." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Requests a verification code for changing password.</summary>
    [HttpPost("me/password/resend-code")]
    public async Task<IActionResult> ResendPasswordCode([FromBody] RequestPasswordChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var ok = await userService.RequestPasswordChangeAsync(userId.Value, request, cancellationToken);
        return ok ? NoContent() : BadRequest(new { message = "Invalid current password or user not found." });
    }

    /// <summary>Confirms password change using a verification code.</summary>
    [HttpPost("me/password/confirm")]
    public async Task<IActionResult> ConfirmPasswordChange([FromBody] ConfirmPasswordChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var ok = await userService.ConfirmPasswordChangeAsync(userId.Value, request, cancellationToken);
        return ok ? NoContent() : BadRequest(new { message = "Invalid or expired verification code." });
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var value) ? value : null;
    }
}
