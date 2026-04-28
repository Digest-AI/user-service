using user_service.Models;

namespace user_service.Interfaces;

public interface IVerificationCodeService
{
    Task<VerificationCode> CreateCodeAsync(Guid userId, string destinationEmail, VerificationCodePurpose purpose, CancellationToken cancellationToken = default);
    Task<bool> ResendEmailCodeAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default);
    Task<bool> ConsumeCodeAsync(Guid userId, string code, VerificationCodePurpose purpose, CancellationToken cancellationToken = default);
}
