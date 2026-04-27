using System.IdentityModel.Tokens.Jwt;
using user_service.Interfaces;
using user_service.Models;

namespace user_service.Services;

public sealed class InternalUserService(IUserRepository userRepository) : IInternalUserService
{
    public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return userRepository.GetUserByIdAsync(userId, cancellationToken);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var handler = new JwtSecurityTokenHandler();
        return handler.CanReadToken(token);
    }
}
