using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.DTOs.Auth;
using user_service.Interfaces;

namespace user_service.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IVerificationCodeService verificationCodeService) : ControllerBase
{
    /// <summary>Starts registration, validates input, and sends a verification code by email.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Accepted(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Resends the registration code if a pending registration exists.</summary>
    [HttpPost("register/resend-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendRegistrationCode([FromBody] ResendRegistrationCodeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.ResendRegistrationCodeAsync(request, cancellationToken);
            return response is null ? NotFound(new { message = "Pending registration not found." }) : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Confirms registration using the email code and creates the account in the database.</summary>
    [HttpPost("register/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await authService.ConfirmRegistrationAsync(request, cancellationToken);
            return user is null ? BadRequest(new { message = "Invalid or expired verification code." }) : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Authenticates a user and issues JWT plus refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid credentials or inactive account." });
        }

        return Ok(result);
    }

    /// <summary>Resends an email verification code to the current email.</summary>
    [HttpPost("resend-email-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailCode([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var ok = await verificationCodeService.ResendEmailCodeAsync(request.Email, cancellationToken);
        return ok ? NoContent() : NotFound(new { message = "User not found." });
    }

    /// <summary>Verifies the user's email using a code.</summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var ok = await verificationCodeService.VerifyEmailAsync(request.Email, request.Code, cancellationToken);
        return ok ? NoContent() : BadRequest(new { message = "Invalid or expired verification code." });
    }

    /// <summary>Refreshes an access token using a refresh token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid refresh token." });
        }

        return Ok(result);
    }

    /// <summary>Revokes a refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var success = await authService.LogoutAsync(userId.Value, request.RefreshToken, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = "Refresh token not found." });
        }

        return NoContent();
    }

    /// <summary>Revokes all refresh tokens for the user.</summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var success = await authService.LogoutAllDevicesAsync(userId.Value, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = "User not found." });
        }

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var value) ? value : null;
    }
}
