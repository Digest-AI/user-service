namespace user_service.DTOs.Auth;

/// <summary>
/// Response when user successfully authenticates with verified email.
/// Contains tokens for API access and session information.
/// </summary>
public sealed class AuthSuccessResponse
{
    /// <summary>
    /// Unique public identifier of the user.
    /// Format: UUID (Globally Unique Identifier)
    /// Usage: Use this to identify the user in the system and for user-related API calls
    /// Example: "550e8400-e29b-41d4-a716-446655440000"
    /// Note: This is different from internal user ID (used only internally)
    /// </summary>
    public required Guid PublicId { get; set; }

    /// <summary>
    /// JWT access token for authenticating API requests.
    /// Usage: Include in Authorization header: "Authorization: Bearer {accessToken}"
    /// Validity: Typically 1 hour from issue time
    /// Format: JWT (JSON Web Token) with encoded claims
    /// Contains: User ID, email, roles, and token metadata
    /// Note: Never expose this to client-side JavaScript; store in HttpOnly cookie for web
    /// Example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Token used to obtain a new access token when the current one expires.
    /// Usage: Send to POST /api/auth/refresh when access token is about to expire
    /// Validity: 
    /// - 7 days if RememberMe = false
    /// - 30 days if RememberMe = true
    /// Security: Must be stored securely (HttpOnly cookie recommended, never in localStorage)
    /// Chain: Part of a refresh token chain for security and audit trail
    /// Important: Keeping this secure is crucial - if compromised, attacker can obtain new access tokens
    /// Example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Exact UTC timestamp when the access token expires.
    /// Format: ISO 8601 datetime (e.g., "2024-12-20T14:30:00Z")
    /// Usage: Use to determine when to refresh access token
    /// Client should: Call POST /api/auth/refresh before this timestamp
    /// Example: "2024-12-20T14:30:00Z"
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// Exact UTC timestamp when the refresh token expires.
    /// Format: ISO 8601 datetime (e.g., "2024-12-27T14:00:00Z")
    /// Usage: After this time, user must log in again (cannot refresh)
    /// Validity Duration:
    /// - 7 days if RememberMe = false
    /// - 30 days if RememberMe = true
    /// Example: "2024-12-27T14:00:00Z"
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Flag indicating if this is a long-term session (RememberMe).
    /// Value:
    /// - true: Extended session on trusted device (refresh token valid 30 days)
    /// - false: Standard session (refresh token valid 7 days)
    /// Usage: Inform user about session duration, disable auto-refresh if false
    /// Security: true means higher security risk if device is compromised
    /// Example: true or false
    /// </summary>
    public bool RememberMe { get; set; }
}
