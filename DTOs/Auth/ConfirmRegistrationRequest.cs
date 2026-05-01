namespace user_service.DTOs.Auth;

/// <summary>
/// Request to confirm registration with verification code.
/// This completes the registration process and creates the user account.
/// </summary>
public sealed class ConfirmRegistrationRequest
{
    /// <summary>
    /// Email address of the account being created.
    /// Requirements:
    /// - Must match the email from the initial POST /api/auth/register request exactly
    /// - Must have a pending registration in the system
    /// - Must not already exist as a confirmed account
    /// Example: "user@example.com"
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// 6-digit verification code sent to the email address.
    /// Details:
    /// - Must be the code received in the confirmation email
    /// - Code is case-insensitive (numeric only)
    /// - Valid for 15 minutes from generation time
    /// - Example: "123456"
    /// Note: If code is incorrect or expired, returns 400 Bad Request
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Flag indicating whether user wants a persistent session (must match initial registration value).
    /// Requirements:
    /// - Should match the RememberMe value from POST /api/auth/register
    /// - If true: refresh token valid for 30 days
    /// - If false: refresh token valid for 7 days (default)
    /// Note: Affects the initial token validity after account confirmation
    /// </summary>
    public bool RememberMe { get; set; }
}
