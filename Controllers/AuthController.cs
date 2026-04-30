using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using user_service.Constants;
using user_service.DTOs.Auth;
using user_service.DTOs.Common;
using user_service.Interfaces;
using user_service.Swagger.Examples;
using user_service.Swagger.Examples.Auth;

namespace user_service.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IVerificationCodeService verificationCodeService) : ControllerBase
{
    /// <summary>Start user registration. Send verification code to email.</summary>
    /// <description>
    /// Step 1 of 3-step registration process.
    /// Creates a pending registration entry and sends a 6-digit verification code to the provided email.
    /// The user must confirm this email with the code to create the account.
    /// </description>
    /// <remarks>
    /// Returns 202 Accepted with temporary registration details.
    /// 
    /// **Error Response (Detail field values):**
    /// - `invalid_email_format` - Email format is invalid (Attr: email)
    /// - `password_too_small` - Password doesn't meet requirements: min 8 chars, at least 1 letter, 1 digit (Attr: password)
    /// - `email_already_exists` - Email already registered (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Accepted(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message, Attr = "email" });
        }
    }

    /// <summary>Resend email verification code for pending registration.</summary>
    /// <description>
    /// If the user didn't receive the verification code or it expired, use this endpoint to get a new one.
    /// Only works for emails that have a pending registration (started but not confirmed).
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `email_not_found` - Email has no pending registration (404 Not Found, Attr: email)
    /// - `email_already_exists` - Email already registered (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPost("register/resend-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResendRegistrationCode([FromBody] ResendRegistrationCodeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.ResendRegistrationCodeAsync(request, cancellationToken);
            return response is null 
                ? NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "email" }) 
                : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message, Attr = "email" });
        }
    }

    /// <summary>Confirm registration using email verification code. Create account.</summary>
    /// <description>
    /// Step 2 of 3-step registration process.
    /// Submit the 6-digit code sent to email to verify email ownership and create the account.
    /// After this, the account is created but email is not yet marked as verified.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_code` - Invalid or expired verification code (400 Bad Request, Attr: code)
    /// - `email_already_exists` - Email already exists (409 Conflict)
    /// </remarks>
    [HttpPost("register/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await authService.ConfirmRegistrationAsync(request, cancellationToken);
            return user is null 
                ? BadRequest(new ErrorResponse { Code = 400, Detail = ErrorCodes.InvalidCode, Attr = "code" }) 
                : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message });
        }
    }

    /// <summary>Authenticate user and get access/refresh tokens.</summary>
    /// <description>
    /// Step 3 of 3-step registration process (after confirm), or standard login.
    /// Authenticates user credentials and returns JWT access token (15 min validity) and refresh token (7 days validity).
    /// Use access token for subsequent API calls. Use refresh token to get new access token when expired.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `password_incorrect` - Invalid email or password (401 Unauthorized, Attr: credentials)
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized(new ErrorResponse { Code = 401, Detail = ErrorCodes.PasswordIncorrect, Attr = "credentials" });
        }

        return Ok(result);
    }

    /// <summary>Send email verification code for email verification without registration.</summary>
    /// <description>
    /// Standalone email verification endpoint (not part of registration flow).
    /// Sends a 6-digit code to verify email ownership. Use this or the registration flow endpoints.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `email_not_found` - Email not registered (404 Not Found, Attr: email)
    /// </remarks>
    [HttpPost("resend-email-code")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendEmailCode([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var ok = await verificationCodeService.ResendEmailCodeAsync(request.Email, cancellationToken);
        return ok ? NoContent() : NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "email" });
    }

    /// <summary>Verify email using verification code.</summary>
    /// <description>
    /// Standalone email verification (completes email verification without registration flow).
    /// Submit the 6-digit code sent to email. Can be used independently of registration.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_code` - Invalid or expired verification code (400 Bad Request, Attr: code)
    /// </remarks>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var ok = await verificationCodeService.VerifyEmailAsync(request.Email, request.Code, cancellationToken);
        return ok ? NoContent() : BadRequest(new ErrorResponse { Code = 400, Detail = ErrorCodes.InvalidCode, Attr = "code" });
    }

    /// <summary>Get new access token using refresh token.</summary>
    /// <description>
    /// Exchange a valid refresh token for a new access token when the current one expires.
    /// Refresh token validity: 7 days. Access token validity: 15 minutes.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_refresh_token` - Invalid or expired refresh token (401 Unauthorized, Attr: refreshToken)
    /// </remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_refresh_token", Attr = "refreshToken" });
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
