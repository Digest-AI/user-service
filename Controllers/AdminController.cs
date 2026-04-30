using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.Constants;
using user_service.DTOs.Admin;
using user_service.DTOs.Common;
using user_service.Interfaces;

namespace user_service.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    /// <summary>Get list of all users.</summary>
    /// <description>
    /// Retrieve list of all users in the system. Admin only endpoint.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `access_denied` - User lacks admin role (403 Forbidden)
    /// </remarks>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetUsersAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Get user details by ID.</summary>
    /// <description>
    /// Retrieve detailed information about a specific user by their ID. Admin only endpoint.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `access_denied` - User lacks admin role (403 Forbidden)
    /// - `email_not_found` - User not found (404 Not Found, Attr: id)
    /// </remarks>
    [HttpGet("users/{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await adminService.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "id" }) : Ok(user);
    }

    /// <summary>Create new user account.</summary>
    /// <description>
    /// Create a new user account with provided credentials and profile info. Admin only endpoint.
    /// Sends verification code to email. User must verify email before account is fully active.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_email_format` - Email format is invalid (400 Bad Request, Attr: email)
    /// - `password_too_small` - Password doesn't meet requirements: min 8 chars, at least 1 letter, 1 digit (400 Bad Request, Attr: password)
    /// - `invalid_age` - Age is invalid (400 Bad Request, Attr: age)
    /// - `invalid_gender` - Gender is invalid (400 Bad Request, Attr: gender)
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `access_denied` - User lacks admin role (403 Forbidden)
    /// - `email_already_exists` - Email already registered (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPost("users")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await adminService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message, Attr = "email" });
        }
    }

    /// <summary>Update user details.</summary>
    /// <description>
    /// Update user's profile information (email, password, name, age, gender, etc.). Admin only endpoint.
    /// Only provided fields will be updated. If email is changed, user must verify new email.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_email_format` - Email format is invalid (400 Bad Request, Attr: email)
    /// - `password_too_small` - Password doesn't meet requirements: min 8 chars, at least 1 letter, 1 digit (400 Bad Request, Attr: password)
    /// - `invalid_age` - Age is invalid (400 Bad Request, Attr: age)
    /// - `invalid_gender` - Gender is invalid (400 Bad Request, Attr: gender)
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `access_denied` - User lacks admin role (403 Forbidden)
    /// - `email_not_found` - User not found (404 Not Found, Attr: id)
    /// - `email_already_exists` - Email already registered (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPut("users/{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await adminService.UpdateUserAsync(id, request, cancellationToken);
            return user is null ? NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "id" }) : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message, Attr = "email" });
        }
    }

    /// <summary>Delete user account permanently.</summary>
    /// <description>
    /// Permanently delete a user account and all associated data. Admin only endpoint.
    /// This action cannot be undone.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `access_denied` - User lacks admin role (403 Forbidden)
    /// - `email_not_found` - User not found (404 Not Found, Attr: id)
    /// </remarks>
    [HttpDelete("users/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await adminService.DeleteUserAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "id" });
    }

    /// <summary>Get list of all available roles.</summary>
    /// <description>
    /// Retrieve all system roles available for user assignment. Admin only endpoint.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `access_denied` - User lacks admin role (403 Forbidden)
    /// </remarks>
    [HttpGet("roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await adminService.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>Set user roles (replace existing roles).</summary>
    /// <description>
    /// Replace user's existing roles with provided role list. Admin only endpoint.
    /// This completely replaces the user's role assignment, not adding to it.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Invalid or expired token (401 Unauthorized)
    /// - `access_denied` - User lacks admin role (403 Forbidden)
    /// - `email_not_found` - User not found (404 Not Found, Attr: id)
    /// </remarks>
    [HttpPut("users/{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRoles([FromRoute] Guid id, [FromBody] SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        var ok = await adminService.SetUserRolesAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "id" });
    }
}
