namespace user_service.DTOs.User;

public sealed class RequestEmailChangeRequest
{
    public required string NewEmail { get; set; }
}

public sealed class ConfirmEmailChangeRequest
{
    public required string NewEmail { get; set; }
    public required string Code { get; set; }
}

public sealed class RequestPasswordChangeRequest
{
    public required string CurrentPassword { get; set; }
}

public sealed class ConfirmPasswordChangeRequest
{
    public required string Code { get; set; }
    public required string NewPassword { get; set; }
}

/// <summary>
/// Response sent after requesting a verification code.
/// Contains the purpose of the verification code.
/// </summary>
public sealed class SendVerificationCodeResponse
{
    public required string Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
}
