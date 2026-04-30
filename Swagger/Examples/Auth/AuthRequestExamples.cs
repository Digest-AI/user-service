using Swashbuckle.AspNetCore.Filters;
using user_service.DTOs.Auth;

namespace user_service.Swagger.Examples.Auth;

public class RegisterRequestExample : IExamplesProvider<RegisterRequest>
{
    public RegisterRequest GetExamples()
    {
        return new RegisterRequest
        {
            Email = "john.doe@example.com",
            Password = "SecurePassword123!"
        };
    }
}

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Email = "john.doe@example.com",
            Password = "SecurePassword123!"
        };
    }
}

public class RefreshRequestExample : IExamplesProvider<RefreshRequest>
{
    public RefreshRequest GetExamples()
    {
        return new RefreshRequest
        {
            RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0..."
        };
    }
}

public class VerifyEmailRequestExample : IExamplesProvider<VerifyEmailRequest>
{
    public VerifyEmailRequest GetExamples()
    {
        return new VerifyEmailRequest
        {
            Email = "john.doe@example.com",
            Code = "123456"
        };
    }
}

public class ConfirmRegistrationRequestExample : IExamplesProvider<ConfirmRegistrationRequest>
{
    public ConfirmRegistrationRequest GetExamples()
    {
        return new ConfirmRegistrationRequest
        {
            Email = "john.doe@example.com",
            Code = "123456"
        };
    }
}

public class ResendRegistrationCodeRequestExample : IExamplesProvider<ResendRegistrationCodeRequest>
{
    public ResendRegistrationCodeRequest GetExamples()
    {
        return new ResendRegistrationCodeRequest
        {
            Email = "john.doe@example.com"
        };
    }
}
