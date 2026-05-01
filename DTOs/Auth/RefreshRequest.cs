namespace user_service.DTOs.Auth;

/// <summary>
/// Request to refresh an expired access token.
/// </summary>
public sealed class RefreshRequest
{
    /// <summary>
    /// The refresh token obtained from login or previous refresh.
    /// Details:
    /// - Must be a valid, non-revoked refresh token from previous authentication
    /// - Must not be expired
    /// - Format: JWT-like token string
    /// Usage: Send this when access token is expired to obtain a new access token
    /// Storage: Must be stored securely (HttpOnly cookie recommended, never in localStorage)
    /// Security: Never expose this to client-side JavaScript
    /// Important: After refresh, old token is revoked and new token should be used for next refresh
    /// Example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// Note: If invalid/expired, returns 401 Unauthorized
    /// </summary>
    public required string RefreshToken { get; set; }
}
