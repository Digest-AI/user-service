namespace user_service.DTOs.Auth;

/// <summary>
/// Request to resend registration verification code.
/// </summary>
public sealed class ResendRegistrationCodeRequest
{
    /// <summary>
    /// Email address used in the initial POST /api/auth/register request.
    /// Requirements:
    /// - Must have an active pending registration (from previous /register call)
    /// - Must not already be confirmed as an account
    /// - Case-insensitive matching
    /// Details:
    /// - A new verification code will be sent to this email
    /// - Previous verification codes will be invalidated
    /// - Code is valid for 15 minutes
    /// Example: "user@example.com"
    /// Note: If no pending registration exists, returns 404 Not Found
    /// </summary>
    public required string Email { get; set; }
}
