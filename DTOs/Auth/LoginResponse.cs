namespace user_service.DTOs.Auth;

/// <summary>
/// Response for login endpoint containing either successful authentication or email verification requirement.
/// The response is polymorphic - contains either authentication tokens OR verification requirement.
/// </summary>
public sealed class LoginResponse
{
    /// <summary>
    /// Contains authentication tokens if user is verified.
    /// Value: null if user email verification is required.
    /// Contents (when not null):
    /// - publicId: Unique user identifier
    /// - accessToken: JWT token for API authentication
    /// - refreshToken: Token to obtain new access tokens
    /// - accessTokenExpiresAt: When access token expires
    /// - refreshTokenExpiresAt: When refresh token expires
    /// - rememberMe: Whether this is a long-term session
    /// Usage: If not null, user is authenticated and can access API
    /// </summary>
    public AuthSuccessResponse? AuthSuccess { get; set; }

    /// <summary>
    /// Contains verification code and details if user email is not verified.
    /// Value: null if user is verified and authenticated.
    /// Contents (when not null):
    /// - email: Email address that needs verification
    /// - verificationCodeExpiresAt: When code expires (15 minutes)
    /// - purpose: Always "verify_email"
    /// - message: Human-readable message
    /// Usage: If not null, user must verify email before accessing system
    /// Next Step: Call POST /api/auth/register/confirm with the verification code
    /// </summary>
    public VerificationRequiredResponse? VerificationRequired { get; set; }

    /// <summary>
    /// Flag indicating if the user's email is verified.
    /// Value:
    /// - true: User is authenticated. Use AuthSuccess properties.
    /// - false: User needs email verification. Use VerificationRequired properties.
    /// Usage: Frontend uses this to determine which response properties to read
    /// Logic:
    /// - If true: AuthSuccess is populated, VerificationRequired is null
    /// - If false: AuthSuccess is null, VerificationRequired is populated
    /// Example: true or false
    /// </summary>
    public bool IsVerified { get; set; }
}
