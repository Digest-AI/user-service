using user_service.Models;

namespace user_service.Interfaces;

public interface IInternalUserService
{
    Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    bool ValidateToken(string token);
}
