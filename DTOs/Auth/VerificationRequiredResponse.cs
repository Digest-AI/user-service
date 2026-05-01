namespace user_service.DTOs.Auth;

/// <summary>
/// Response when user needs to verify their email.
/// This is returned from login if the user hasn't verified their email yet,
/// or when initiating password restoration.
/// </summary>
public sealed class VerificationRequiredResponse
{
    /// <summary>
    /// Email address that needs to be verified.
    /// Details:
    /// - The registered email address
    /// - A verification code has been sent to this address
    /// - User must confirm they have access to this email
    /// Example: "user@example.com"
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Exact UTC timestamp when the verification code expires.
    /// Format: ISO 8601 datetime (e.g., "2024-12-20T13:45:00Z")
    /// Details:
    /// - Code is valid for 15 minutes
    /// - User must enter code before this timestamp
    /// - If expired, user must request a new code via POST /api/auth/register/resend-code
    /// Usage: Display to user as deadline for code entry
    /// Example: "2024-12-20T13:45:00Z"
    /// </summary>
    public DateTime VerificationCodeExpiresAt { get; set; }

    /// <summary>
    /// Purpose of the verification code.
    /// Value: "verify_email"
    /// Meaning: User needs to confirm email ownership (during registration or login)
    /// Usage: Frontend uses this to display appropriate message and determine next API call
    /// Next Step: Call POST /api/auth/register/confirm with the verification code
    /// Example: "verify_email"
    /// </summary>
    public required string Purpose { get; set; }

    /// <summary>
    /// Human-readable message for the client.
    /// Provides context about what the user needs to do.
    /// Default value: "Email verification required."
    /// Usage: Display this message to inform user about verification requirement
    /// Example: "Email verification required."
    /// </summary>
    public string Message { get; set; } = "Email verification required.";
}
