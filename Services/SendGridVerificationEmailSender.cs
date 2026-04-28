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
        var client = new SendGridClient(_options.ApiKey);
        var from = new EmailAddress(_options.FromEmail, _options.FromName ?? "Digest.AI");
        var subject = purpose switch
        {
            VerificationCodePurpose.EMAIL => "Email verification code",
            VerificationCodePurpose.BACKUP_EMAIL => "Backup email verification code",
            VerificationCodePurpose.PASSWORD => "Password verification code",
            _ => "Verification code"
        };

        _logger.LogInformation(
            "Sending SendGrid email verification message. To={ToEmail}, Purpose={Purpose}, From={FromEmail}, Subject={Subject}",
            toEmail,
            purpose,
            _options.FromEmail,
            subject);

        var message = MailHelper.CreateSingleEmail(
            from,
            new EmailAddress(toEmail),
            subject,
            $"Your verification code is: {code}",
            $"Your verification code is: {code}");

        try
        {
            var response = await client.SendEmailAsync(message, cancellationToken);
            var body = await response.Body.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SendGrid returned non-success response. To={ToEmail}, Purpose={Purpose}, StatusCode={StatusCode}, Body={Body}",
                    toEmail,
                    purpose,
                    response.StatusCode,
                    body);
                return;
            }

            _logger.LogInformation(
                "SendGrid email sent successfully. To={ToEmail}, Purpose={Purpose}, StatusCode={StatusCode}",
                toEmail,
                purpose,
                response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "SendGrid email send was cancelled. To={ToEmail}, Purpose={Purpose}",
                toEmail,
                purpose);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "SendGrid email send failed. To={ToEmail}, Purpose={Purpose}, From={FromEmail}",
                toEmail,
                purpose,
                _options.FromEmail);
            throw;
        }
    }
}
