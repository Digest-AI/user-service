namespace user_service.DTOs.Auth;

/// <summary>
/// Response sent when registration or password restoration verification is initiated.
/// Contains information about the verification code sent to user's email.
/// </summary>
public sealed class VerificationInitiationResponse
{
    /// <summary>
    /// Email address where the verification code was sent.
    /// Details:
    /// - The address provided in the registration request
    /// - Normalized to lowercase
    /// - Displayed to user for confirmation
    /// Example: "user@example.com"
    /// Usage: Show to user to confirm the email address where code was sent
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Exact UTC time when the verification code expires.
    /// Format: ISO 8601 datetime (e.g., "2024-12-20T13:45:00Z")
    /// Details:
    /// - Code is valid for exactly 15 minutes from generation
    /// - After this time, code cannot be used
    /// - User must call resend-code endpoint to get a new code
    /// Usage: Display to user as deadline for code entry
    /// Example: "2024-12-20T13:45:00Z"
    /// </summary>
    public DateTime VerificationCodeExpiresAt { get; set; }

    /// <summary>
    /// Purpose of this verification code.
    /// Possible values:
    /// - "verify_email": User is registering or verifying email during login
    ///   Next step: Call POST /api/auth/register/confirm with the code
    /// Usage: Frontend uses to show appropriate UI message and determine flow
    /// Example: "verify_email"
    /// </summary>
    public required string Purpose { get; set; }

    /// <summary>
    /// Human-readable status message for the client.
    /// Provides context about the next steps.
    /// Default value: "Verification code sent."
    /// Examples:
    /// - "Verification code sent. Confirm registration to create the account."
    /// - "Verification code resent."
    /// Usage: Display to user to explain what they need to do
    /// </summary>
    public string Message { get; set; } = "Verification code sent.";
}
