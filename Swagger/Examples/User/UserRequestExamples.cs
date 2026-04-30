using Swashbuckle.AspNetCore.Filters;
using user_service.DTOs.User;

namespace user_service.Swagger.Examples.User;

public class UpdateProfileRequestExample : IExamplesProvider<UpdateProfileRequest>
{
    public UpdateProfileRequest GetExamples()
    {
        return new UpdateProfileRequest
        {
            Name = "Jane",
            Surname = "Smith",
            Age = 28,
            Gender = "Female"
        };
    }
}

public class RequestEmailChangeRequestExample : IExamplesProvider<RequestEmailChangeRequest>
{
    public RequestEmailChangeRequest GetExamples()
    {
        return new RequestEmailChangeRequest
        {
            NewEmail = "jane.smith@example.com"
        };
    }
}

public class ConfirmEmailChangeRequestExample : IExamplesProvider<ConfirmEmailChangeRequest>
{
    public ConfirmEmailChangeRequest GetExamples()
    {
        return new ConfirmEmailChangeRequest
        {
            NewEmail = "jane.smith@example.com",
            Code = "123456"
        };
    }
}

public class RequestPasswordChangeRequestExample : IExamplesProvider<RequestPasswordChangeRequest>
{
    public RequestPasswordChangeRequest GetExamples()
    {
        return new RequestPasswordChangeRequest
        {
            CurrentPassword = "OldPassword123!"
        };
    }
}

public class ConfirmPasswordChangeRequestExample : IExamplesProvider<ConfirmPasswordChangeRequest>
{
    public ConfirmPasswordChangeRequest GetExamples()
    {
        return new ConfirmPasswordChangeRequest
        {
            Code = "123456",
            NewPassword = "NewPassword123!"
        };
    }
}

public class ChangePasswordRequestExample : IExamplesProvider<ChangePasswordRequest>
{
    public ChangePasswordRequest GetExamples()
    {
        return new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };
    }
}
