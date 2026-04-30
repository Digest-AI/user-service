namespace user_service.Constants;

/// <summary>
/// Standard error codes for API responses.
/// </summary>
public static class ErrorCodes
{
    // Email validation errors
    public const string InvalidEmailFormat = "invalid_email_format";
    public const string EmailAlreadyExists = "email_already_exists";
    public const string EmailNotFound = "email_not_found";
    public const string EmailTheSame = "email_the_same";

    // Password validation errors
    public const string PasswordMismatch = "password_mismatch";
    public const string PasswordTooSmall = "password_too_small";
    public const string PasswordOnlyNumbers = "password_only_numbers";
    public const string PasswordOnlyLetters = "password_only_letters";
    public const string PasswordIncorrect = "password_incorrect";
    public const string PasswordNotSupported = "password_not_supported";
    public const string PasswordTheSame = "password_the_same";

    // Verification code errors
    public const string InvalidCode = "invalid_code";
    public const string CodeTooShort = "code_too_short";
}
