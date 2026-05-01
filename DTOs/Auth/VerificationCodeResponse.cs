namespace user_service.DTOs.Auth;

/// <summary>
/// Response sent when password restoration is initiated.
/// Contains information about the verification code sent for password recovery.
/// </summary>
public sealed class VerificationCodeResponse
{
    /// <summary>
    /// Email address where the verification code was sent.
    /// Details:
    /// - The email address of the account being recovered
    /// - Normalized to lowercase
    /// - Displayed to user for confirmation
    /// Example: "user@example.com"
    /// Usage: Show to user to confirm the email where recovery code was sent
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Exact UTC time when the verification code expires.
    /// Format: ISO 8601 datetime (e.g., "2024-12-20T13:45:00Z")
    /// Details:
    /// - Code is valid for exactly 15 minutes from generation
    /// - After this time, code cannot be used for password restoration
    /// - User must call resend-code endpoint to get a new code
    /// Usage: Display to user as deadline for entering the code
    /// Example: "2024-12-20T13:45:00Z"
    /// </summary>
    public DateTime VerificationCodeExpiresAt { get; set; }

    /// <summary>
    /// Purpose of this verification code.
    /// Value: "restore_password"
    /// Meaning: User is restoring their password.
    /// Usage: Frontend uses to display appropriate UI and flow control
    /// Next Step: Call POST /api/auth/restore/confirm with code and new password
    /// Example: "restore_password"
    /// </summary>
    public required string Purpose { get; set; }

    /// <summary>
    /// Human-readable status message for the client.
    /// Provides context about password restoration process.
    /// Default value: "Verification code sent."
    /// Examples:
    /// - "Verification code sent. Use it to restore your password."
    /// - "Verification code resent."
    /// Usage: Display to user to explain password restoration flow
    /// </summary>
    public string Message { get; set; } = "Verification code sent.";
}
