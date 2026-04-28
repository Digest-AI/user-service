using System.Net.Mail;

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
            throw new InvalidOperationException("Email is required.");
        }

        var normalized = email.Trim().ToLowerInvariant();
        try
        {
            var mailAddress = new MailAddress(normalized);
            if (!string.Equals(mailAddress.Address, normalized, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid email.");
            }
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Invalid email.");
        }

        return normalized;
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
}
