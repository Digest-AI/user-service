namespace user_service.DTOs.Auth;

/// <summary>
/// Request to confirm password restoration with new password.
/// This completes the password recovery process.
/// </summary>
public sealed class ConfirmRestorePasswordRequest
{
    /// <summary>
    /// Email address of the account whose password is being restored.
    /// Requirements:
    /// - Must match an account in the system
    /// - Must match the email from POST /api/auth/restore request
    /// - Case-insensitive matching
    /// Example: "user@example.com"
    /// Note: If email doesn't exist, returns 400 Bad Request
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// 6-digit verification code sent to the email.
    /// Details:
    /// - Code received in the password restoration email
    /// - Must be entered within 15 minutes of receiving
    /// - Case-insensitive (numeric only)
    /// - Format: 6 digits (e.g., "123456")
    /// Validity: 15 minutes from generation time
    /// Note: If code is incorrect or expired, returns 400 Bad Request
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// The new password for the account.
    /// Requirements:
    /// - Minimum 8 characters long
    /// - Must contain at least 1 digit (0-9)
    /// - Case-sensitive
    /// Examples of valid passwords: "NewPass123", "Secure99", "Update2024"
    /// Examples of invalid passwords: 
    /// - "NoDigits" (no digits)
    /// - "123456" (no letters)
    /// - "Short1" (less than 8 characters)
    /// After restoration: User can log in with this password immediately
    /// Note: If password doesn't meet requirements, returns 400 Bad Request
    /// </summary>
    public required string NewPassword { get; set; }
}
