namespace user_service.DTOs.Auth;

/// <summary>
/// Response when login requires email verification.
/// Contains only the purpose of verification.
/// </summary>
public sealed class LoginVerificationRequiredResponse
{
    /// <summary>
    /// Purpose of the verification required.
    /// Possible values:
    /// - "verify_email": User needs to verify their email before full login
    /// - "restore_password": User needs to restore their password
    /// Usage: Frontend uses this to determine which flow to show user
    /// Next Step: User will receive verification code at their registered email
    /// Example: "verify_email"
    /// </summary>
    public required string Purpose { get; set; }
}
