using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.DTOs.Auth;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Services;

public sealed class AuthService(UserServiceDbContext dbContext, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthUserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedUsername = request.Username.Trim();

        if (await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        if (await dbContext.Users.AnyAsync(x => x.Username == normalizedUsername, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Username = normalizedUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            TelegramChatId = request.TelegramChatId,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var defaultRole = await dbContext.Roles.SingleAsync(x => x.Name == "USER", cancellationToken);

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id,
            IsDeleted = false
        });

        user.Preferences = new Preference
        {
            Id = Guid.NewGuid(),
            UserId = user.Id
        };

        user.NotificationSettings = new NotificationSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EmailEnabled = true,
            TelegramEnabled = true,
            PushEnabled = true
        };

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildAuthUserDtoAsync(user.Id, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return null;
        }

        var roles = user.UserRoles.Select(x => x.Role.Name).ToArray();
        var (accessToken, accessExpiresAt) = jwtTokenService.CreateAccessToken(user, roles);
        var (rawRefreshToken, refreshTokenHash, refreshExpiresAt) = jwtTokenService.CreateRefreshToken(request.RememberMe);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        user.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            Revoked = false,
            IsDeleted = false,
            RememberMe = request.RememberMe
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            UserId = user.Id,
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
            .SingleOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (refreshToken is null || refreshToken.Revoked || refreshToken.IsDeleted || refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        if (refreshToken.User.RefreshTokensRevokedAt.HasValue && refreshToken.CreatedAt <= refreshToken.User.RefreshTokensRevokedAt.Value)
        {
            return null;
        }

        var user = refreshToken.User;
        if (!user.IsActive)
        {
            return null;
        }

        refreshToken.Revoked = true;
        refreshToken.IsDeleted = true;
        refreshToken.DeletedAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(x => x.Role.Name).ToArray();
        var (accessToken, accessExpiresAt) = jwtTokenService.CreateAccessToken(user, roles);
        var (rawRefreshToken, newRefreshHash, refreshExpiresAt) = jwtTokenService.CreateRefreshToken(refreshToken.RememberMe);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            Revoked = false,
            IsDeleted = false,
            RememberMe = refreshToken.RememberMe
        });

        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            UserId = user.Id,
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

        user.RefreshTokensRevokedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AuthUserDto> BuildAuthUserDtoAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => new AuthUserDto
            {
                Id = x.Id,
                Email = x.Email,
                Username = x.Username,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Phone = x.Phone,
                TelegramChatId = x.TelegramChatId,
                IsActive = x.IsActive,
                IsVerified = x.IsVerified,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                LastLoginAt = x.LastLoginAt,
                Roles = x.UserRoles.Select(ur => ur.Role.Name).ToArray()
            })
            .SingleAsync(cancellationToken);
    }
}
