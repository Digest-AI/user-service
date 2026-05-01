namespace user_service.DTOs.Auth;

/// <summary>
/// Request to authenticate a user and receive tokens.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// User's email address registered in the system.
    /// Requirements:
    /// - Must be an exact match to registered email (case-insensitive matching)
    /// - Must be a registered account
    /// Example: "user@example.com"
    /// Note: If email not found, returns 401 Unauthorized
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's password for authentication.
    /// Requirements:
    /// - Must match the password set during registration (exact match after hashing)
    /// - Case-sensitive (uppercase and lowercase matter)
    /// Note: If password is incorrect, returns 401 Unauthorized
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Flag indicating whether user wants a persistent session (RememberMe functionality).
    /// Impact on refresh token:
    /// - If true: refresh token valid for 30 days (extended validity for persistent sessions)
    /// - If false: refresh token valid for 7 days (standard validity for security)
    /// Default value: false
    /// Recommendation: 
    /// - Use true for desktop/native applications (auto-login on trusted devices)
    /// - Use false for web browsers and public devices
    /// Security consideration: Longer token validity means higher compromise risk if device is lost
    /// </summary>
    public bool RememberMe { get; set; }
}
