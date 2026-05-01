namespace user_service.DTOs.Auth;

/// <summary>
/// Error response for failed auth API calls.
/// Contains information about what went wrong.
/// </summary>
public sealed class AuthErrorResponse
{
    /// <summary>
    /// Human-readable error message describing what went wrong.
    /// Usage: Display to user or log for debugging
    /// Examples:
    /// - "Email already exists."
    /// - "Invalid credentials or inactive account."
    /// - "Invalid or expired verification code."
    /// - "User not authenticated."
    /// Typical HTTP Status Codes:
    /// - 400 Bad Request: Invalid input format or expired code
    /// - 401 Unauthorized: Authentication failed or access token expired
    /// - 404 Not Found: Resource not found (e.g., pending registration not found)
    /// - 409 Conflict: Business logic violation (e.g., email already exists)
    /// Note: Error messages should not expose sensitive system information
    /// </summary>
    public required string Message { get; set; }
}
