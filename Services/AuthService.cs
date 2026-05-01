using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using user_service.Data;
using user_service.DTOs.Auth;
using user_service.Interfaces;
using user_service.Models;
using user_service.Validation;

namespace user_service.Services;

public sealed class AuthService(
    UserServiceDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IVerificationCodeService verificationCodeService,
    IVerificationEmailSender emailSender) : IAuthService
{
    public async Task<VerificationInitiationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
        UserInputValidation.ValidatePassword(request.Password);

        if (await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("email_already_exists");
        }

        var code = Random.Shared.Next(0, 1_000_000).ToString("D6");
        var pending = await dbContext.PendingRegistrations.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (pending is null)
        {
            pending = new PendingRegistration
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RememberMe = request.RememberMe
            };
            await dbContext.PendingRegistrations.AddAsync(pending, cancellationToken);
        }
        else
        {
            pending.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            pending.Code = code;
            pending.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
            pending.RememberMe = request.RememberMe;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await emailSender.SendCodeAsync(pending.Email, pending.Code, VerificationCodePurpose.EMAIL, cancellationToken);

        return new VerificationInitiationResponse
        {
            Email = pending.Email,
            VerificationCodeExpiresAt = pending.ExpiresAt,
            Purpose = "verify_email",
            Message = "Verification code sent. Confirm registration to create the account."
        };
    }

    public async Task<VerificationInitiationResponse?> ResendRegistrationCodeAsync(ResendRegistrationCodeRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
        var pending = await dbContext.PendingRegistrations.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (pending is null)
        {
            return null;
        }

        if (await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("email_already_exists");
        }

        pending.Code = Random.Shared.Next(0, 1_000_000).ToString("D6");
        pending.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailSender.SendCodeAsync(pending.Email, pending.Code, VerificationCodePurpose.EMAIL, cancellationToken);

        return new VerificationInitiationResponse
        {
            Email = pending.Email,
            VerificationCodeExpiresAt = pending.ExpiresAt,
            Purpose = "verify_email",
            Message = "Verification code resent."
        };
    }

    public async Task<AuthSuccessResponse?> ConfirmRegistrationAsync(ConfirmRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
        var pending = await dbContext.PendingRegistrations.SingleOrDefaultAsync(
            x => x.Email == normalizedEmail && x.Code == request.Code && x.ExpiresAt > DateTime.UtcNow,
            cancellationToken);

        if (pending is null)
        {
            return null;
        }

        if (await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            PublicId = Guid.NewGuid(),
            Email = pending.Email,
            PasswordHash = pending.PasswordHash,
            IsVerified = true,
            DateJoined = DateTime.UtcNow,
            Name = string.Empty,
            Surname = string.Empty,
            Age = 0,
            Gender = "unknown",
            IsDeleted = false
        };

        var defaultRole = await dbContext.Roles.SingleAsync(x => x.Name == "USER", cancellationToken);
        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id,
            IsDeleted = false
        });

        var chain = new Chain
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RememberMe = pending.RememberMe
        };

        dbContext.Chains.Add(chain);
        dbContext.PendingRegistrations.Remove(pending);
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = new[] { "USER" };
        var (accessToken, accessExpiresAt) = jwtTokenService.CreateAccessToken(user, roles);
        var (rawRefreshToken, refreshTokenHash, refreshExpiresAt) = jwtTokenService.CreateRefreshToken(pending.RememberMe);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ChainId = chain.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            Revoked = false,
            IsDeleted = false,
            RememberMe = pending.RememberMe
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthSuccessResponse
        {
            PublicId = user.PublicId,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt,
            RememberMe = pending.RememberMe
        };
    }

    public async Task<VerificationCodeResponse> RestorePasswordAsync(RestorePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User with this email not found.");
        }

        var code = await verificationCodeService.CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.PASSWORD, cancellationToken);

        return new VerificationCodeResponse
        {
            Email = user.Email,
            VerificationCodeExpiresAt = code.ExpiresAt,
            Purpose = "restore_password",
            Message = "Verification code sent. Use it to restore your password."
        };
    }

    public async Task<VerificationCodeResponse?> ResendRestorePasswordCodeAsync(ResendRestorePasswordCodeRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var code = await verificationCodeService.CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.PASSWORD, cancellationToken);

        return new VerificationCodeResponse
        {
            Email = user.Email,
            VerificationCodeExpiresAt = code.ExpiresAt,
            Purpose = "restore_password",
            Message = "Verification code resent."
        };
    }

    public async Task<bool> ConfirmRestorePasswordAsync(ConfirmRestorePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);
        UserInputValidation.ValidatePassword(request.NewPassword);

        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var consumed = await verificationCodeService.ConsumeCodeAsync(user.Id, request.Code, VerificationCodePurpose.PASSWORD, cancellationToken);
        if (!consumed)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.IsVerified = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = UserInputValidation.NormalizeEmail(request.Email);

        var user = await dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            return null;
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return null;
        }

        // Если пользователь не верифицирован, отправляем код подтверждения
        if (!user.IsVerified)
        {
            var code = await verificationCodeService.CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.EMAIL, cancellationToken);
            return new LoginResponse
            {
                IsVerified = false,
                VerificationRequired = new VerificationRequiredResponse
                {
                    Email = user.Email,
                    VerificationCodeExpiresAt = code.ExpiresAt,
                    Purpose = "verify_email",
                    Message = "Email verification required. Please check your email for the verification code."
                }
            };
        }

        var chain = new Chain
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RememberMe = request.RememberMe
        };

        dbContext.Chains.Add(chain);

        var roles = user.UserRoles.Select(x => x.Role.Name).ToArray();
        var (accessToken, accessExpiresAt) = jwtTokenService.CreateAccessToken(user, roles);
        var (rawRefreshToken, refreshTokenHash, refreshExpiresAt) = jwtTokenService.CreateRefreshToken(request.RememberMe);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ChainId = chain.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            Revoked = false,
            IsDeleted = false,
            RememberMe = request.RememberMe
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            IsVerified = true,
            AuthSuccess = new AuthSuccessResponse
            {
                PublicId = user.PublicId,
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshTokenExpiresAt = refreshExpiresAt,
                RememberMe = request.RememberMe
            }
        };
    }

    public async Task<AuthSuccessResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = jwtTokenService.ComputeRefreshTokenHash(request.RefreshToken);

        var refreshToken = await dbContext.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Include(x => x.Chain)
            .SingleOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (refreshToken is null || refreshToken.Revoked || refreshToken.IsDeleted || refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        var user = refreshToken.User;
        if (user.IsDeleted)
        {
            return null;
        }

        refreshToken.Revoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.IsDeleted = true;
        refreshToken.DeletedAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(x => x.Role.Name).ToArray();
        var (accessToken, accessExpiresAt) = jwtTokenService.CreateAccessToken(user, roles);
        var (rawRefreshToken, newRefreshHash, refreshExpiresAt) = jwtTokenService.CreateRefreshToken(refreshToken.RememberMe);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ChainId = refreshToken.ChainId,
            TokenHash = newRefreshHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            Revoked = false,
            IsDeleted = false,
            RememberMe = refreshToken.RememberMe
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthSuccessResponse
        {
            PublicId = user.PublicId,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt,
            RememberMe = refreshToken.RememberMe
        };
    }

    public async Task<bool> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return true;
        }

        var hash = jwtTokenService.ComputeRefreshTokenHash(refreshToken);
        var token = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.UserId == userId && x.TokenHash == hash, cancellationToken);

        if (token is null)
        {
            return true;
        }

        token.Revoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.IsDeleted = true;
        token.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> LogoutAllDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var refreshTokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.IsDeleted = true;
            token.DeletedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AuthUserDto> BuildAuthUserDtoAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => new AuthUserDto
            {
                PublicId = x.PublicId,
                Email = x.Email,
                Name = x.Name,
                Surname = x.Surname,
                Age = x.Age,
                Gender = x.Gender,
                IsVerified = x.IsVerified,
                DateJoined = x.DateJoined,
                Roles = x.UserRoles.Select(ur => ur.Role.Name).ToArray()
            })
            .SingleAsync(cancellationToken);
    }
}
