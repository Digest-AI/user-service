using user_service.Models;

namespace user_service.Interfaces;

public interface IVerificationEmailSender
{
    Task SendCodeAsync(string toEmail, string code, VerificationCodePurpose purpose, CancellationToken cancellationToken = default);
}
