using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.Interfaces;

namespace user_service.Controllers;

[ApiController]
[Route("api/internal/users")]
public sealed class InternalController(IInternalUserService internalService) : ControllerBase
{
    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await internalService.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [AllowAnonymous]
    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
    {
        var valid = internalService.ValidateToken(request.Token);
        return Ok(new { valid });
    }
}

public sealed class ValidateTokenRequest
{
    public required string Token { get; set; }
}
