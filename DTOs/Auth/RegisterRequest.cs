namespace user_service.DTOs.Auth;

/// <summary>
/// Request to start user registration.
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// User's email address.
    /// Requirements:
    /// - Must be a valid email format (contains @ and domain)
    /// - Must not be already registered in the system
    /// - Case-insensitive (converted to lowercase for storage)
    /// Example: "user@example.com"
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's password for account security.
    /// Requirements:
    /// - Minimum 8 characters long
    /// - Must contain at least 1 digit (0-9)
    /// Examples of valid passwords: "MyPass123", "Test1234", "Secure99"
    /// Examples of invalid passwords: "NoDigits", "123456" (no letters), "Short1" (too short)
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Flag indicating whether user wants a persistent session (RememberMe functionality).
    /// If true: refresh token validity extended to 30 days (for long-term sessions on trusted devices)
    /// If false: refresh token validity set to 7 days (default, recommended for public devices)
    /// Default value: false
    /// Use case: true for desktop/native apps, false for web browsers and public devices
    /// </summary>
    public bool RememberMe { get; set; }
}
