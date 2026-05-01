using System.Net.Mail;
using System.Text.RegularExpressions;

namespace user_service.Validation;

public static class UserInputValidation
{
    private static readonly HashSet<string> AllowedGenders = new(StringComparer.OrdinalIgnoreCase)
    {
        "male",
        "female",
        "other",
        "unknown",
        "prefer_not_to_say"
    };

    public static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("invalid_email_format");
        }

        var normalized = email.Trim().ToLowerInvariant();
        try
        {
            var mailAddress = new MailAddress(normalized);
            if (!string.Equals(mailAddress.Address, normalized, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("invalid_email_format");
            }
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("invalid_email_format");
        }

        return normalized;
    }

    public static string ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (password.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters long.");
        }

        if (!password.Any(char.IsDigit))
        {
            throw new InvalidOperationException("Password must contain at least one digit.");
        }

        return password;
    }

    public static string ValidateNewPassword(string currentPassword, string newPassword)
    {
        ValidatePassword(newPassword);

        if (string.Equals(currentPassword, newPassword))
        {
            throw new InvalidOperationException("password_the_same");
        }

        return newPassword;
    }

    public static int ValidateAge(int age)
    {
        if (age is < 0 or > 120)
        {
            throw new InvalidOperationException("Age must be between 0 and 120.");
        }

        return age;
    }

    public static string ValidateGender(string gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
        {
            throw new InvalidOperationException("Gender is required.");
        }

        var normalized = gender.Trim();
        if (normalized.Length is < 1 or > 32)
        {
            throw new InvalidOperationException("Gender is invalid.");
        }

        if (!AllowedGenders.Contains(normalized))
        {
            throw new InvalidOperationException("Gender is invalid.");
        }

        return normalized;
    }

    public static string ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("code_too_short");
        }

        if (code.Length != 6 || !Regex.IsMatch(code, @"^\d{6}$"))
        {
            throw new InvalidOperationException("code_too_short");
        }

        return code;
    }

    public static string ValidateNewEmail(string currentEmail, string newEmail)
    {
        var normalizedNew = NormalizeEmail(newEmail);

        if (string.Equals(currentEmail, normalizedNew, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("email_the_same");
        }

        return normalizedNew;
    }
}
