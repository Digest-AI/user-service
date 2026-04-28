using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Services;

public sealed class VerificationCodeService(UserServiceDbContext dbContext, IVerificationEmailSender emailSender) : IVerificationCodeService
{
    public async Task<VerificationCode> CreateCodeAsync(Guid userId, string destinationEmail, VerificationCodePurpose purpose, CancellationToken cancellationToken = default)
    {
        var code = Random.Shared.Next(0, 1_000_000).ToString("D6");

        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        dbContext.VerificationCodes.Add(verificationCode);
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailSender.SendCodeAsync(destinationEmail, code, purpose, cancellationToken);
        return verificationCode;
    }

    public async Task<bool> ResendEmailCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return false;
        }

        await CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.EMAIL, cancellationToken);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var consumed = await ConsumeCodeAsync(user.Id, code, VerificationCodePurpose.EMAIL, cancellationToken);
        if (!consumed)
        {
            return false;
        }

        user.IsVerified = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ConsumeCodeAsync(Guid userId, string code, VerificationCodePurpose purpose, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var matchedCode = await dbContext.VerificationCodes
            .Where(x => x.UserId == userId && x.Purpose == purpose && x.ExpiresAt > now)
            .OrderByDescending(x => x.ExpiresAt)
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

        if (matchedCode is null)
        {
            return false;
        }

        var relatedCodes = await dbContext.VerificationCodes
            .Where(x => x.UserId == userId && x.Purpose == purpose)
            .ToListAsync(cancellationToken);

        dbContext.VerificationCodes.RemoveRange(relatedCodes);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
