namespace user_service.DTOs.Auth;

/// <summary>
/// Generic success response for operations that don't return specific data.
/// Used for operations like password restoration, logout, etc.
/// </summary>
public sealed class SuccessResponse
{
    /// <summary>
    /// Human-readable message describing the operation result.
    /// Usage: Display to user to confirm operation was successful
    /// Examples:
    /// - "Password restored successfully." (after password restoration)
    /// - "Logout successful." (after logout)
    /// - Any other operation completion message
    /// Note: Message is localized to the client's language/region
    /// </summary>
    public required string Message { get; set; }
}
