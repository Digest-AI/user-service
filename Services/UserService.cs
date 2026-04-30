using BCrypt.Net;
using user_service.DTOs.User;
using user_service.Interfaces;
using user_service.Models;
using user_service.Validation;

namespace user_service.Services;

public sealed class UserService(IUserRepository userRepository, IVerificationCodeService verificationCodeService) : IUserService
{
    public async Task<UserProfileDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(userId, cancellationToken);
        return user is null ? null : MapProfile(user);
    }

    public async Task<UserProfileDto?> UpdateMeAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserWithRolesAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        var surname = request.Surname.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 128)
        {
            throw new InvalidOperationException("Name is invalid.");
        }

        if (string.IsNullOrWhiteSpace(surname) || surname.Length > 128)
        {
            throw new InvalidOperationException("Surname is invalid.");
        }

        user.Name = name;
        user.Surname = surname;
        user.Age = UserInputValidation.ValidateAge(request.Age);
        user.Gender = UserInputValidation.ValidateGender(request.Gender);
        await userRepository.SaveChangesAsync(cancellationToken);
        return MapProfile(user);
    }

    public async Task<bool> RequestEmailChangeAsync(Guid userId, RequestEmailChangeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var normalizedEmail = UserInputValidation.NormalizeEmail(request.NewEmail);
        if (await userRepository.EmailExistsAsync(normalizedEmail, userId, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        await verificationCodeService.CreateCodeAsync(user.Id, normalizedEmail, VerificationCodePurpose.EMAIL, cancellationToken);
        return true;
    }

    public async Task<bool> ConfirmEmailChangeAsync(Guid userId, ConfirmEmailChangeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var normalizedEmail = UserInputValidation.NormalizeEmail(request.NewEmail);
        if (await userRepository.EmailExistsAsync(normalizedEmail, userId, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var consumed = await verificationCodeService.ConsumeCodeAsync(user.Id, request.Code, VerificationCodePurpose.EMAIL, cancellationToken);
        if (!consumed)
        {
            return false;
        }

        user.Email = normalizedEmail;
        user.IsVerified = true;
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RequestPasswordChangeAsync(Guid userId, RequestPasswordChangeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        await verificationCodeService.CreateCodeAsync(user.Id, user.Email, VerificationCodePurpose.PASSWORD, cancellationToken);
        return true;
    }

    public async Task<bool> ConfirmPasswordChangeAsync(Guid userId, ConfirmPasswordChangeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
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
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.DateDeleted = DateTime.UtcNow;
        user.IsDeleted = true;

        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserProfileDto MapProfile(User user)
    {
        return new UserProfileDto
        {
            PublicId = user.PublicId,
            Email = user.Email,
            Name = user.Name,
            Surname = user.Surname,
            Age = user.Age,
            Gender = user.Gender,
            IsVerified = user.IsVerified,
            DateJoined = user.DateJoined,
            DateDeleted = user.DateDeleted,
            IsDeleted = user.IsDeleted,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToArray()
        };
    }
}
