using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using user_service.Constants;
using user_service.DTOs.Auth;
using user_service.DTOs.Common;
using user_service.Interfaces;
using user_service.Swagger.Examples;
using user_service.Swagger.Examples.Auth;

namespace user_service.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService, IVerificationCodeService verificationCodeService) : ControllerBase
{
<<<<<<< Updated upstream
    /// <summary>Start user registration. Send verification code to email.</summary>
    /// <description>
    /// Step 1 of 3-step registration process.
    /// Creates a pending registration entry and sends a 6-digit verification code to the provided email.
    /// The user must confirm this email with the code to create the account.
    /// </description>
    /// <remarks>
    /// Returns 202 Accepted with temporary registration details.
    /// 
    /// **Error Response (Detail field values):**
    /// - `invalid_email_format` - Email format is invalid (Attr: email)
    /// - `password_too_small` - Password doesn't meet requirements: min 8 chars, at least 1 letter, 1 digit (Attr: password)
    /// - `email_already_exists` - Email already registered (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
=======
    /// <summary>
    /// Starts user registration by validating email and password, then sends a verification code to the email.
    /// </summary>
    /// <remarks>
    /// **Step 1 of Registration Process**
    /// 
    /// This endpoint initiates the registration flow. It:
    /// 1. Validates email format (must be valid email)
    /// 2. Validates password (minimum 8 characters, at least 1 digit)
    /// 3. Checks if email is not already registered
    /// 4. Generates and sends a 6-digit verification code to the email
    /// 5. Returns the purpose of verification code
    /// 
    /// **Requirements:**
    /// - Email: Must be a valid email format and not already registered
    /// - Password: Minimum 8 characters, must contain at least 1 digit (e.g., "MyPass123")
    /// - RememberMe: Boolean flag for extended session duration (optional, defaults to false)
    /// 
    /// **Verification Code:**
    /// - Valid for 15 minutes
    /// - Sent to the provided email address
    /// - 6-digit numeric code
    /// 
    /// **Next Step:** Call POST /api/auth/register/confirm with the verification code
    /// </remarks>
    /// <response code="202">Verification code sent successfully. User should check their email for the code and proceed to /register/confirm endpoint.</response>
    /// <response code="409">Email already registered, invalid email format, or weak password.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PurposeResponse), StatusCodes.Status202Accepted)]
>>>>>>> Stashed changes
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Accepted(new PurposeResponse { Purpose = response.Purpose });
        }
        catch (InvalidOperationException ex)
        {
<<<<<<< Updated upstream
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message, Attr = "email" });
        }
    }

    /// <summary>Resend email verification code for pending registration.</summary>
    /// <description>
    /// If the user didn't receive the verification code or it expired, use this endpoint to get a new one.
    /// Only works for emails that have a pending registration (started but not confirmed).
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `email_not_found` - Email has no pending registration (404 Not Found, Attr: email)
    /// - `email_already_exists` - Email already registered (409 Conflict, Attr: email)
    /// </remarks>
    [HttpPost("register/resend-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
=======
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Resends the registration verification code to the email address.
    /// </summary>
    /// <remarks>
    /// **Resend verification code for registration**
    /// 
    /// Use this endpoint if:
    /// - User didn't receive the initial verification code
    /// - The code has expired (15-minute validity)
    /// - User wants to receive a new code
    /// 
    /// **Important:** 
    /// - Only works if there's an active pending registration for this email
    /// - Previous verification codes become invalid when a new one is issued
    /// - This endpoint can only be used during the registration process (before account confirmation)
    /// 
    /// **Next Step:** Call POST /api/auth/register/confirm with the new verification code
    /// </remarks>
    /// <response code="200">Verification code resent successfully to the email address.</response>
    /// <response code="404">No pending registration found for this email. User must start with POST /register first.</response>
    /// <response code="409">Email already registered or validation failed.</response>
    [HttpPost("register/resend-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PurposeResponse), StatusCodes.Status200OK)]
>>>>>>> Stashed changes
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResendRegistrationCode([FromBody] ResendRegistrationCodeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.ResendRegistrationCodeAsync(request, cancellationToken);
<<<<<<< Updated upstream
            return response is null 
                ? NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "email" }) 
                : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message, Attr = "email" });
        }
    }

    /// <summary>Confirm registration using email verification code. Create account.</summary>
    /// <description>
    /// Step 2 of 3-step registration process.
    /// Submit the 6-digit code sent to email to verify email ownership and create the account.
    /// After this, the account is created but email is not yet marked as verified.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_code` - Invalid or expired verification code (400 Bad Request, Attr: code)
    /// - `email_already_exists` - Email already exists (409 Conflict)
    /// </remarks>
    [HttpPost("register/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
=======
            return response is null ? NotFound(new ErrorResponse { Message = "Pending registration not found." }) : Ok(new PurposeResponse { Purpose = response.Purpose });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Completes the registration process and creates the user account.
    /// </summary>
    /// <remarks>
    /// **Step 2 of Registration Process (Final Step)**
    /// 
    /// This endpoint finalizes registration by:
    /// 1. Validating the verification code (must be correct and not expired)
    /// 2. Creating the user account with the provided email and password
    /// 3. Marking the email as verified
    /// 4. Assigning default "USER" role
    /// 5. Creating a refresh token chain for session management
    /// 6. Issuing access and refresh tokens
    /// 7. Automatically logging in the user
    /// 
    /// **Requirements:**
    /// - Email: Must match the email from the initial POST /register request
    /// - Code: 6-digit verification code received in email (valid for 15 minutes from issue time)
    /// - RememberMe: Boolean flag (must match the value from initial registration)
    /// 
    /// **Upon Success:**
    /// - User account is created with IsVerified = true
    /// - Access token is issued (typically valid for 1 hour)
    /// - Refresh token is issued:
    ///   - 7 days validity if RememberMe = false
    ///   - 30 days validity if RememberMe = true
    /// - User can immediately use access token for authenticated requests
    /// 
    /// **Response contains:**
    /// - publicId: User's unique public identifier
    /// - accessToken: JWT token for API authentication
    /// - refreshToken: Token to obtain new access tokens when expired
    /// - accessTokenExpiresAt: UTC timestamp when access token expires
    /// - refreshTokenExpiresAt: UTC timestamp when refresh token expires
    /// - rememberMe: Boolean flag indicating session duration
    /// 
    /// **Next Step:** Use accessToken in Authorization header for API requests, or call POST /api/auth/refresh when access token expires
    /// </remarks>
    /// <response code="200">Account created successfully. User is now authenticated and can use the provided tokens.</response>
    /// <response code="400">Invalid or expired verification code. User must call POST /register/resend-code to get a new code.</response>
    /// <response code="409">Email already exists in the system, or validation failed.</response>
    [HttpPost("register/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthSuccessResponse), StatusCodes.Status200OK)]
>>>>>>> Stashed changes
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
<<<<<<< Updated upstream
            var user = await authService.ConfirmRegistrationAsync(request, cancellationToken);
            return user is null 
                ? BadRequest(new ErrorResponse { Code = 400, Detail = ErrorCodes.InvalidCode, Attr = "code" }) 
                : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Code = 409, Detail = ex.Message });
        }
    }

    /// <summary>Authenticate user and get access/refresh tokens.</summary>
    /// <description>
    /// Step 3 of 3-step registration process (after confirm), or standard login.
    /// Authenticates user credentials and returns JWT access token (15 min validity) and refresh token (7 days validity).
    /// Use access token for subsequent API calls. Use refresh token to get new access token when expired.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `password_incorrect` - Invalid email or password (401 Unauthorized, Attr: credentials)
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
=======
            var response = await authService.ConfirmRegistrationAsync(request, cancellationToken);
            return response is null ? BadRequest(new ErrorResponse { Message = "Invalid or expired verification code." }) : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user and issues access and refresh tokens.
    /// </summary>
    /// <remarks>
    /// **User Authentication Endpoint**
    /// 
    /// This endpoint authenticates users with email and password. The response varies based on email verification status.
    /// 
    /// **Requirements:**
    /// - Email: Must be registered in the system (case-insensitive)
    /// - Password: Must match the registered password exactly
    /// - RememberMe: Boolean flag for extended session duration
    /// 
    /// **Behavior:**
    /// 
    /// **Case 1: User is verified (normal case)**
    /// - Returns AuthSuccessResponse with all authentication tokens
    /// - Access token is issued for API authentication
    /// - Refresh token is issued for obtaining new access tokens
    /// - User can immediately use the tokens
    /// 
    /// **Case 2: User is not verified (registration incomplete)**
    /// - Returns VerificationRequiredResponse instead
    /// - Email verification code is sent to user's email
    /// - User must verify email before accessing the system
    /// - Common scenario: User started registration but didn't complete confirmation
    /// 
    /// **Tokens details:**
    /// - Access Token: Valid for ~1 hour, use in Authorization: Bearer {accessToken}
    /// - Refresh Token: 
    ///   - 7 days validity if RememberMe = false
    ///   - 30 days validity if RememberMe = true
    /// - New refresh token chain is created per login (for security/audit trail)
    /// 
    /// **Next Steps:**
    /// - If isVerified = true: Use accessToken for authenticated API requests
    /// - If isVerified = false: Call POST /api/auth/register/confirm with the code sent to email
    /// </remarks>
    /// <response code="200">Authentication successful. Response contains either authenticated tokens (if verified) or verification requirement (if not verified).</response>
    /// <response code="401">Invalid credentials: email not found or password incorrect.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
>>>>>>> Stashed changes
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (result is null)
        {
<<<<<<< Updated upstream
            return Unauthorized(new ErrorResponse { Code = 401, Detail = ErrorCodes.PasswordIncorrect, Attr = "credentials" });
=======
            return Unauthorized(new ErrorResponse { Message = "Invalid credentials or inactive account." });
>>>>>>> Stashed changes
        }

        return Ok(result);
    }

<<<<<<< Updated upstream
    /// <summary>Send email verification code for email verification without registration.</summary>
    /// <description>
    /// Standalone email verification endpoint (not part of registration flow).
    /// Sends a 6-digit code to verify email ownership. Use this or the registration flow endpoints.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `email_not_found` - Email not registered (404 Not Found, Attr: email)
    /// </remarks>
    [HttpPost("resend-email-code")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendEmailCode([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var ok = await verificationCodeService.ResendEmailCodeAsync(request.Email, cancellationToken);
        return ok ? NoContent() : NotFound(new ErrorResponse { Code = 404, Detail = ErrorCodes.EmailNotFound, Attr = "email" });
    }

    /// <summary>Verify email using verification code.</summary>
    /// <description>
    /// Standalone email verification (completes email verification without registration flow).
    /// Submit the 6-digit code sent to email. Can be used independently of registration.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_code` - Invalid or expired verification code (400 Bad Request, Attr: code)
    /// </remarks>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var ok = await verificationCodeService.VerifyEmailAsync(request.Email, request.Code, cancellationToken);
        return ok ? NoContent() : BadRequest(new ErrorResponse { Code = 400, Detail = ErrorCodes.InvalidCode, Attr = "code" });
    }

    /// <summary>Get new access token using refresh token.</summary>
    /// <description>
    /// Exchange a valid refresh token for a new access token when the current one expires.
    /// Refresh token validity: 7 days. Access token validity: 15 minutes.
    /// </description>
    /// <remarks>
    /// **Error Response (Detail field values):**
    /// - `invalid_refresh_token` - Invalid or expired refresh token (401 Unauthorized, Attr: refreshToken)
    /// </remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
=======
    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// **Token Refresh Endpoint**
    /// 
    /// When the access token expires, use this endpoint to obtain a new access token without requiring user credentials.
    /// This extends the user session seamlessly.
    /// 
    /// **Requirements:**
    /// - RefreshToken: Must be a valid, non-expired, and non-revoked refresh token from a previous login or refresh
    /// 
    /// **Process:**
    /// 1. Validates the refresh token
    /// 2. Checks if token is not revoked or deleted
    /// 3. Checks if token has not expired
    /// 4. Issues a new access token
    /// 5. Issues a new refresh token (maintaining same chain)
    /// 6. Revokes the old refresh token (for security)
    /// 7. Returns both new tokens
    /// 
    /// **Important Security Notes:**
    /// - Old refresh token is immediately revoked after issuing new token
    /// - Each refresh operation invalidates the previous refresh token
    /// - Refresh tokens maintain a chain ID for audit trail
    /// - RememberMe flag from original token is preserved in new token
    /// - If chain gets compromised, all tokens in the chain can be revoked via POST /logout-all
    /// 
    /// **Response contains:**
    /// - publicId: User's unique public identifier
    /// - accessToken: New JWT token for API authentication
    /// - refreshToken: New token for future refreshes
    /// - accessTokenExpiresAt: UTC timestamp when new access token expires
    /// - refreshTokenExpiresAt: UTC timestamp when new refresh token expires
    /// - rememberMe: Boolean flag indicating session duration
    /// 
    /// **Next Step:** Use the new accessToken for API requests, save the new refreshToken for future refreshes
    /// </remarks>
    /// <response code="200">New tokens issued successfully. Old refresh token is revoked.</response>
    /// <response code="401">Invalid, expired, revoked, or malformed refresh token. User must log in again.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthSuccessResponse), StatusCodes.Status200OK)]
>>>>>>> Stashed changes
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        if (result is null)
        {
<<<<<<< Updated upstream
            return Unauthorized(new ErrorResponse { Code = 401, Detail = "invalid_refresh_token", Attr = "refreshToken" });
=======
            return Unauthorized(new ErrorResponse { Message = "Invalid refresh token." });
>>>>>>> Stashed changes
        }

        return Ok(result);
    }

    /// <summary>
    /// Initiates password restoration process by sending a verification code to the user's email.
    /// </summary>
    /// <remarks>
    /// **Step 1 of Password Restoration Process**
    /// 
    /// This endpoint starts the password recovery flow for users who forgot their password. It:
    /// 1. Finds the user by email
    /// 2. Generates a 6-digit verification code
    /// 3. Sends the code to the registered email address
    /// 4. Returns the purpose of verification
    /// 
    /// **Requirements:**
    /// - Email: Must be registered in the system
    /// 
    /// **Security Note:**
    /// - If the email doesn't exist, the endpoint still returns 202 (for security - prevents email enumeration)
    /// - User won't receive any email if email is not registered, but response appears successful
    /// 
    /// **Verification Code:**
    /// - Valid for 15 minutes
    /// - Sent to the registered email address
    /// - 6-digit numeric code
    /// - Purpose: "restore_password"
    /// 
    /// **Next Step:** Call POST /api/auth/restore/confirm with the new password and verification code
    /// </remarks>
    /// <response code="202">Password restoration initiated. Verification code sent to email (if email exists).</response>
    /// <response code="409">Validation failed (empty email, invalid format, etc.).</response>
    [HttpPost("restore")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PurposeResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RestorePassword([FromBody] RestorePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RestorePasswordAsync(request, cancellationToken);
            return Accepted(new PurposeResponse { Purpose = response.Purpose });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Resends the password restoration verification code to the user's email.
    /// </summary>
    /// <remarks>
    /// **Resend verification code for password restoration**
    /// 
    /// Use this endpoint if:
    /// - User didn't receive the initial verification code
    /// - The code has expired (15-minute validity)
    /// - User wants to receive a new code
    /// 
    /// **Requirements:**
    /// - Email: Must be registered in the system
    /// 
    /// **Important:**
    /// - Previous verification codes become invalid when a new one is issued
    /// - Code is valid for 15 minutes from the time it was generated
    /// 
    /// **Next Step:** Call POST /api/auth/restore/confirm with the new verification code and new password
    /// </remarks>
    /// <response code="200">Verification code resent successfully to the email address.</response>
    /// <response code="404">User with this email not found.</response>
    /// <response code="409">Validation failed.</response>
    [HttpPost("restore/resend-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PurposeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResendRestorePasswordCode([FromBody] ResendRestorePasswordCodeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.ResendRestorePasswordCodeAsync(request, cancellationToken);
            return response is null ? NotFound(new ErrorResponse { Message = "User not found." }) : Ok(new PurposeResponse { Purpose = response.Purpose });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Completes the password restoration process by verifying the code and setting a new password.
    /// </summary>
    /// <remarks>
    /// **Step 2 of Password Restoration Process (Final Step)**
    /// 
    /// This endpoint finalizes password recovery by:
    /// 1. Finding the user by email
    /// 2. Validating the verification code (must be correct and not expired)
    /// 3. Validating the new password requirements
    /// 4. Updating the user's password
    /// 5. Marking the user as verified (email confirmed valid)
    /// 6. Invalidating the verification code
    /// 
    /// **Requirements:**
    /// - Email: Must match the email from POST /restore request
    /// - Code: 6-digit verification code received in email (valid for 15 minutes)
    /// - NewPassword: Minimum 8 characters, must contain at least 1 digit (e.g., "NewPass123")
    /// 
    /// **Upon Success:**
    /// - User's password is updated and hashed
    /// - User is marked as verified
    /// - All previous refresh tokens remain valid (user can continue using old sessions)
    /// - User can log in with the new password
    /// 
    /// **Password Requirements:**
    /// - Minimum 8 characters
    /// - At least 1 digit
    /// - Examples of valid passwords: "NewPass123", "Secure99", "Update2024"
    /// 
    /// **Next Step:** Log in with POST /api/auth/login using the new password
    /// </remarks>
    /// <response code="200">Password restored successfully. User can now log in with the new password.</response>
    /// <response code="400">Invalid or expired verification code, or invalid password format.</response>
    /// <response code="409">Validation failed (email not found, etc.).</response>
    [HttpPost("restore/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmRestorePassword([FromBody] ConfirmRestorePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await authService.ConfirmRestorePasswordAsync(request, cancellationToken);
            return success 
                ? Ok(new SuccessResponse { Message = "Password restored successfully." }) 
                : BadRequest(new ErrorResponse { Message = "Invalid or expired verification code." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// </summary>
    /// <remarks>
    /// **Logout from Current Device/Session**
    /// 
    /// This endpoint logs out the user from the current device by revoking the specific refresh token.
    /// 
    /// **Requirements:**
    /// - Authentication: User must be authenticated (access token required)
    /// - RefreshToken: The refresh token from the current session (optional in request body)
    /// 
    /// **Behavior:**
    /// - Revokes only the specified refresh token
    /// - Current access token remains valid until its expiration
    /// - Other active sessions (other devices/browsers) remain active
    /// - If RememberMe sessions exist on other devices, they continue working
    /// 
    /// **Security Notes:**
    /// - Endpoint is idempotent - calling multiple times is safe
    /// - Revoked tokens cannot be used for refresh operations
    /// - User can continue with access token until it expires
    /// - After logout, user cannot refresh the session (will get 401 on refresh attempt)
    /// 
    /// **For Complete Logout:**
    /// - Use POST /api/auth/logout-all to revoke all devices/sessions
    /// - Use POST /api/auth/logout-all to end RememberMe sessions on all devices
    /// 
    /// **Next Step:** If only logging out current device, user can continue on other devices. To prevent that, call POST /logout-all
    /// </remarks>
    /// <response code="204">Logout successful. Current session has been revoked.</response>
    /// <response code="401">User not authenticated. Access token is missing or invalid.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest? request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(new ErrorResponse { Message = "User not authenticated." });
        }

        var refreshToken = request?.RefreshToken ?? string.Empty;
        await authService.LogoutAsync(userId.Value, refreshToken, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Logs out a user from all devices by revoking all their sessions.
    /// </summary>
    /// <remarks>
    /// **Complete Logout from All Devices**
    /// 
    /// This endpoint performs a complete logout by revoking all refresh tokens for the user across all devices.
    /// 
    /// **Requirements:**
    /// - Authentication: User must be authenticated (access token required)
    /// 
    /// **Behavior:**
    /// - Revokes ALL refresh tokens for this user
    /// - Current access token remains valid until expiration
    /// - All other devices/sessions lose their refresh tokens
    /// - All RememberMe sessions are terminated
    /// - User cannot refresh from any device after this operation
    /// 
    /// **Use Cases:**
    /// - User suspects account compromise
    /// - User wants to ensure complete security
    /// - User wants to end all RememberMe sessions on other devices
    /// - User is changing password and wants to invalidate all sessions
    /// 
    /// **Security Notes:**
    /// - Most secure logout option
    /// - All active refresh token chains are terminated
    /// - Users on other devices will get 401 when trying to refresh tokens
    /// - User can still use access token on current device until it expires
    /// - After access token expires, user must log in again from all devices
    /// 
    /// **Important:**
    /// - No refresh token parameter is required or used
    /// - Endpoint doesn't fail if refresh token is not provided
    /// - Only requires valid access token (Authorization header)
    /// 
    /// **Next Step:** User must log in again (POST /api/auth/login) from any device
    /// </remarks>
    /// <response code="204">All sessions revoked successfully from all devices.</response>
    /// <response code="401">User not authenticated. Access token is missing or invalid.</response>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(new ErrorResponse { Message = "User not authenticated." });
        }

        await authService.LogoutAllDevicesAsync(userId.Value, cancellationToken);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var value) ? value : null;
    }
}
