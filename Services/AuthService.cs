using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
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
    public async Task<RegistrationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
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
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            await dbContext.PendingRegistrations.AddAsync(pending, cancellationToken);
        }
        else
        {
            pending.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            pending.Code = code;
            pending.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await emailSender.SendCodeAsync(pending.Email, pending.Code, VerificationCodePurpose.EMAIL, cancellationToken);

        return new RegistrationResponse
        {
            PublicId = pending.Id,
            Email = pending.Email,
            VerificationCodeExpiresAt = pending.ExpiresAt,
            Message = "Verification code sent."
        };
    }

    public async Task<RegistrationResponse?> ResendRegistrationCodeAsync(ResendRegistrationCodeRequest request, CancellationToken cancellationToken = default)
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

        return new RegistrationResponse
        {
            PublicId = pending.Id,
            Email = pending.Email,
            VerificationCodeExpiresAt = pending.ExpiresAt,
            Message = "Verification code resent."
        };
    }

    public async Task<AuthUserDto?> ConfirmRegistrationAsync(ConfirmRegistrationRequest request, CancellationToken cancellationToken = default)
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

        dbContext.PendingRegistrations.Remove(pending);
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildAuthUserDtoAsync(user.Id, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
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

        await verificationCodeService.CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.EMAIL, cancellationToken);

        return new AuthResponse
        {
            PublicId = user.PublicId,
            ChainId = chain.Id,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task<AuthResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
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

        return new AuthResponse
        {
            PublicId = user.PublicId,
            ChainId = refreshToken.ChainId,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task<bool> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = jwtTokenService.ComputeRefreshTokenHash(refreshToken);
        var token = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.UserId == userId && x.TokenHash == hash, cancellationToken);

        if (token is null)
        {
            return false;
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
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.DateDeleted = DateTime.UtcNow;
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
