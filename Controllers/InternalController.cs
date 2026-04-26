using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_service.Interfaces;

namespace user_service.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/internal/users")]
public sealed class InternalController(IInternalUserService internalService) : ControllerBase
{
    [HttpGet("{id:guid}/preferences")]
    public async Task<IActionResult> GetPreferences(Guid id, CancellationToken cancellationToken)
    {
        var result = await internalService.GetPreferencesAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/actions")]
    public async Task<IActionResult> GetActions(Guid id, CancellationToken cancellationToken)
    {
        var result = await internalService.GetActionsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/notification-settings")]
    public async Task<IActionResult> GetNotificationSettings(Guid id, CancellationToken cancellationToken)
    {
        var result = await internalService.GetNotificationSettingsAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/telegram")]
    public async Task<IActionResult> GetTelegram(Guid id, CancellationToken cancellationToken)
    {
        var result = await internalService.GetTelegramAsync(id, cancellationToken);
        return Ok(new { telegramChatId = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await internalService.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [AllowAnonymous]
    [HttpGet("validate-token")]
    public IActionResult ValidateToken([FromQuery] string token)
    {
        var valid = internalService.ValidateToken(token);
        return Ok(new { valid });
    }
}
