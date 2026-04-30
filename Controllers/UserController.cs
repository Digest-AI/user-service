using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using user_service.Constants;
using user_service.DTOs.Common;
using user_service.DTOs.User;
using user_service.Interfaces;
using user_service.Swagger.Examples;
using user_service.Swagger.Examples.User;

namespace user_service.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public sealed class UserController(IUserService userService) : ControllerBase
{
    /// <summary>Get current user profile.</summary>
    /// <description>
    /// Retrieve authenticated user's profile information (personal data, roles, verification status).
    /// Requires valid access token.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `email_not_found` - User profile not found (404 Not Found)
    /// </remarks>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_token" });

        var profile = await userService.GetMeAsync(userId.Value, cancellationToken);
        return profile is null ? NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound }) : Ok(profile);
    }

    /// <summary>Update current user profile.</summary>
    /// <description>
    /// Update user's personal information (name, surname, age, gender, email).
    /// Only provided fields will be updated. Requires valid access token.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_email_format` - Email format is invalid (400 Bad Request, Attr: email)
    /// - `invalid_age` - Age is invalid (400 Bad Request, Attr: age)
    /// - `invalid_gender` - Gender is invalid (400 Bad Request, Attr: gender)
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `email_not_found` - User not found (404 Not Found)
    /// </remarks>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_token" });

        var profile = await userService.UpdateMeAsync(userId.Value, request, cancellationToken);
        return profile is null ? NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound }) : Ok(profile);
    }

    /// <summary>Request email change verification code.</summary>
    /// <description>
    /// Step 1 of email change process. Send 6-digit verification code to the new email address.
    /// User must confirm with code to complete email change.
    /// Requires valid access token.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `email_not_found` - User not found (404 Not Found)
    /// - `email_already_exists` - New email already in use (409 Conflict, Attr: email)
    /// - `email_the_same` - New email is same as current (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPost("me/email/resend-code")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResendEmailCode([FromBody] RequestEmailChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_token" });

        try
        {
            var ok = await userService.RequestEmailChangeAsync(userId.Value, request, cancellationToken);
            return ok ? NoContent() : NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message, Attr = "email" });
        }
    }

    /// <summary>Confirm email change with verification code.</summary>
    /// <description>
    /// Step 2 of email change process. Submit 6-digit code sent to new email to complete the change.
    /// After confirmation, user's email is updated and marked as verified.
    /// Requires valid access token.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_code` - Invalid or expired verification code (400 Bad Request, Attr: code)
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `email_already_exists` - Email already in use (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPost("me/email/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_token" });

        try
        {
            var ok = await userService.ConfirmEmailChangeAsync(userId.Value, request, cancellationToken);
            return ok ? NoContent() : BadRequest(new ErrorResponse { Code = 400, Detail = ErrorCodes.InvalidCode, Attr = "code" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message });
        }
    }

    /// <summary>Request password change verification code.</summary>
    /// <description>
    /// Step 1 of password change process. Verify current password and send 6-digit code to user's email.
    /// User must confirm with code to complete password change.
    /// Requires valid access token.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `password_incorrect` - Current password is incorrect (400 Bad Request, Attr: password)
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// </remarks>
    [HttpPost("me/password/resend-code")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResendPasswordCode([FromBody] RequestPasswordChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_token" });

        var ok = await userService.RequestPasswordChangeAsync(userId.Value, request, cancellationToken);
        return ok ? NoContent() : BadRequest(new ErrorResponse { Code = 400, Detail = ErrorCodes.PasswordIncorrect, Attr = "password" });
    }

    /// <summary>Confirm password change with verification code.</summary>
    /// <description>
    /// Step 2 of password change process. Submit 6-digit code sent to email with new password to complete change.
    /// New password must meet requirements: min 8 chars, at least 1 letter, 1 digit.
    /// Requires valid access token.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_code` - Invalid or expired verification code (400 Bad Request, Attr: code)
    /// - `password_the_same` - New password is same as current (400 Bad Request, Attr: password)
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// </remarks>
    [HttpPost("me/password/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmPasswordChange([FromBody] ConfirmPasswordChangeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_token" });

        var ok = await userService.ConfirmPasswordChangeAsync(userId.Value, request, cancellationToken);
        return ok ? NoContent() : BadRequest(new ErrorResponse { Code = 400, Detail = ErrorCodes.InvalidCode, Attr = "code" });
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var value) ? value : null;
    }
}
