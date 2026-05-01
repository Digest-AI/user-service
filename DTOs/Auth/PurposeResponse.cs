namespace user_service.DTOs.Auth;

/// <summary>
/// Simple response indicating the purpose of verification code sent to email.
/// Returned after sending verification code to user's email.
/// </summary>
public sealed class PurposeResponse
{
    /// <summary>
    /// Purpose of the verification code that was sent to the email.
    /// Possible values:
    /// - "verify_email": User needs to verify email during registration or login attempt
    ///   Next step: Call POST /api/auth/register/confirm with the code
    /// - "restore_password": User is restoring their password
    ///   Next step: Call POST /api/auth/restore/confirm with the code and new password
    /// Usage: Frontend uses this to determine which confirmation flow to show user
    /// Example: "verify_email" or "restore_password"
    /// </summary>
    public required string Purpose { get; set; }
}
