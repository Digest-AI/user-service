namespace user_service.DTOs.Auth;

/// <summary>
/// Request to resend password restoration verification code.
/// </summary>
public sealed class ResendRestorePasswordCodeRequest
{
    /// <summary>
    /// Email address of the registered user account.
    /// Requirements:
    /// - Must be registered in the system
    /// - Case-insensitive matching
    /// - Must be a valid email format
    /// Details:
    /// - New verification code will be sent to this email
    /// - Previous verification codes will be invalidated
    /// - Code is valid for 15 minutes
    /// Example: "user@example.com"
    /// Note: If user not found, returns 404 Not Found
    /// </summary>
    public required string Email { get; set; }
}
