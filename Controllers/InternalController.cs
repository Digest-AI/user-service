using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.Constants;
using user_service.DTOs.Admin;
using user_service.DTOs.Common;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/internal")]
public sealed class InternalController(IInternalUserService internalUserService) : ControllerBase
{
    /// <summary>Get user by ID (internal service use only).</summary>
    /// <description>
    /// Retrieve user information for internal microservice communication. Not exposed in public API.
    /// Requires valid access token for internal services.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `email_not_found` - User not found (404 Not Found, Attr: id)
    /// </remarks>
    [HttpGet("users/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await internalUserService.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "id" }) : Ok(user);
    }

    /// <summary>Validate JWT token (internal service use only).</summary>
    /// <description>
    /// Validate a JWT token and get user claims. Not exposed in public API.
    /// Used by internal services to verify token validity without calling user endpoints.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_token` - Token is invalid or expired (401 Unauthorized, Attr: token)
    /// </remarks>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
    {
        var isValid = internalUserService.ValidateToken(request.Token);
        return isValid ? Ok(true) : Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_token", Attr = "token" });
    }
}

public sealed class ValidateTokenRequest
{
    public required string Token { get; set; }
}
