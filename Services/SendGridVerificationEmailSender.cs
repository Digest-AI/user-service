using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using user_service.Interfaces;
using user_service.Models;
using user_service.Options;

namespace user_service.Services;

public sealed class SendGridVerificationEmailSender(IOptions<SendGridOptions> options, ILogger<SendGridVerificationEmailSender> logger) : IVerificationEmailSender
{
    private readonly SendGridOptions _options = options.Value;
    private readonly ILogger<SendGridVerificationEmailSender> _logger = logger;

    public async Task SendCodeAsync(string toEmail, string code, VerificationCodePurpose purpose, CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogError("Cannot send verification code: recipient email is empty");
            throw new ArgumentException("Recipient email cannot be empty", nameof(toEmail));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogError("Cannot send verification code: code is empty");
            throw new ArgumentException("Verification code cannot be empty", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("Cannot send verification code: SendGrid API key is not configured");
            throw new InvalidOperationException("SendGrid API key is not configured");
        }

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            _logger.LogError("Cannot send verification code: SendGrid FromEmail is not configured");
            throw new InvalidOperationException("SendGrid FromEmail is not configured");
        }

        var subject = purpose switch
        {
            VerificationCodePurpose.EMAIL => "Email Verification Code",
            VerificationCodePurpose.PASSWORD => "Password Reset Code",
            VerificationCodePurpose.BACKUP_EMAIL => "Backup Email Verification Code",
            _ => "Verification Code"
        };

        var htmlContent = BuildHtmlEmail(code, subject);
        var textContent = BuildTextEmail(code, subject);

        _logger.LogInformation(
            "Sending SendGrid email verification. To={ToEmail}, Purpose={Purpose}, From={FromEmail}",
            toEmail,
            purpose,
            _options.FromEmail);

        var client = new SendGridClient(_options.ApiKey);
        var from = new EmailAddress(_options.FromEmail, _options.FromName ?? "Digest.AI");
        var message = MailHelper.CreateSingleEmail(
            from,
            new EmailAddress(toEmail),
            subject,
            textContent,
            htmlContent);

        try
        {
            var response = await client.SendEmailAsync(message, cancellationToken);
            var body = await response.Body.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SendGrid email failed. To={ToEmail}, Purpose={Purpose}, StatusCode={StatusCode}, Response={Response}",
                    toEmail,
                    purpose,
                    response.StatusCode,
                    body);
                throw new InvalidOperationException($"SendGrid returned status code {response.StatusCode}: {body}");
            }

            _logger.LogInformation(
                "SendGrid email sent successfully. To={ToEmail}, Purpose={Purpose}, StatusCode={StatusCode}",
                toEmail,
                purpose,
                response.StatusCode);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex,
                "SendGrid email send was cancelled. To={ToEmail}, Purpose={Purpose}",
                toEmail,
                purpose);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SendGrid email send failed with exception. To={ToEmail}, Purpose={Purpose}, From={FromEmail}",
                toEmail,
                purpose,
                _options.FromEmail);
            throw;
        }
    }

    private static string BuildTextEmail(string code, string subject)
    {
        return $@"{subject}

Your verification code is: {code}

This code will expire in 15 minutes.
Do not share this code with anyone.

If you did not request this verification, please ignore this email.";
    }

    private static string BuildHtmlEmail(string code, string subject)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; border-radius: 5px 5px 0 0; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .code-box {{ background-color: #e8f4f8; border: 2px solid #007bff; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0; }}
        .code {{ font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #007bff; font-family: monospace; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; text-align: center; }}
        .warning {{ color: #dc3545; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{subject}</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            <p>You requested a verification code to secure your Digest.AI account. Here is your code:</p>
            <div class=""code-box"">
                <div class=""code"">{code}</div>
            </div>
            <p><strong>This code will expire in 15 minutes.</strong></p>
            <p class=""warning"">⚠️ Do not share this code with anyone. Digest.AI support staff will never ask for this code.</p>
            <p>If you did not request this verification, please ignore this email or contact our support team.</p>
            <div class=""footer"">
                <p>This is an automated message, please do not reply to this email.</p>
                <p>&copy; 2024 Digest.AI. All rights reserved.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
