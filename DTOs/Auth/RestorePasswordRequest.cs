namespace user_service.DTOs.Auth;

/// <summary>
/// Request to initiate password restoration process.
/// </summary>
public sealed class RestorePasswordRequest
{
    /// <summary>
    /// Email address of the account whose password needs to be restored.
    /// Requirements:
    /// - Must be registered in the system
    /// - Case-insensitive (converted to lowercase for lookup)
    /// - Must be a valid email format
    /// Details:
    /// - If email exists: Verification code will be sent
    /// - If email doesn't exist: No error is returned (for security - prevents email enumeration)
    /// - User will not receive email if address is not registered
    /// Example: "user@example.com"
    /// Note: Even if no account exists, endpoint returns 202 (success) for security
    /// </summary>
    public required string Email { get; set; }
}
