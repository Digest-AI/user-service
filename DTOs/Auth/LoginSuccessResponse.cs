namespace user_service.DTOs.Auth;

/// <summary>
/// Response when login is successful and user is verified.
/// Contains all authentication tokens needed for API access.
/// </summary>
public sealed class LoginSuccessResponse
{
    /// <summary>
    /// Unique public identifier of the user.
    /// Format: UUID (Globally Unique Identifier)
    /// Usage: Use this to identify the user in the system
    /// Example: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// </summary>
    public required Guid PublicId { get; set; }

    /// <summary>
    /// JWT access token for authenticating API requests.
    /// Usage: Include in Authorization header: "Authorization: Bearer {accessToken}"
    /// Validity: Typically 1 hour from issue time
    /// Example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Token used to obtain a new access token when the current one expires.
    /// Usage: Send to POST /api/auth/refresh to get new access token
    /// Validity: 
    /// - 7 days if RememberMe = false
    /// - 30 days if RememberMe = true
    /// Format: Secure random token
    /// Example: "550e8400-e29b-41d4-a716-446655440000"
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Exact UTC timestamp when the access token expires.
    /// Format: ISO 8601 datetime (e.g., "2026-05-01T16:49:52.570Z")
    /// Usage: Client should refresh token before this time
    /// Example: "2026-05-01T16:49:52.570Z"
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// Exact UTC timestamp when the refresh token expires.
    /// Format: ISO 8601 datetime (e.g., "2026-05-01T16:49:52.570Z")
    /// Usage: After this time, user must login again
    /// Example: "2026-05-01T16:49:52.570Z"
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Flag indicating if this is a long-term session.
    /// Value:
    /// - true: Extended session (30 days refresh token validity)
    /// - false: Standard session (7 days refresh token validity)
    /// Usage: Used to determine token refresh behavior
    /// Example: true or false
    /// </summary>
    public required bool RememberMe { get; set; }
}
