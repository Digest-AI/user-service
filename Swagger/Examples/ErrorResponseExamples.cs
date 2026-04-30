using Swashbuckle.AspNetCore.Filters;
using user_service.Constants;
using user_service.DTOs.Common;

namespace user_service.Swagger.Examples;

public class ErrorResponseExample : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples()
    {
        return new ErrorResponse
        {
            Code = 400,
            Detail = ErrorCodes.InvalidEmailFormat,
            Attr = "email"
        };
    }
}

public class ErrorUnauthorizedExample : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples()
    {
        return new ErrorResponse
        {
            Code = 401,
            Detail = ErrorCodes.PasswordIncorrect,
            Attr = null
        };
    }
}

public class ErrorForbiddenExample : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples()
    {
        return new ErrorResponse
        {
            Code = 403,
            Detail = "access_denied",
            Attr = null
        };
    }
}

public class ErrorNotFoundExample : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples()
    {
        return new ErrorResponse
        {
            Code = 404,
            Detail = ErrorCodes.EmailNotFound,
            Attr = "id"
        };
    }
}

public class ErrorConflictExample : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples()
    {
        return new ErrorResponse
        {
            Code = 409,
            Detail = ErrorCodes.EmailAlreadyExists,
            Attr = "email"
        };
    }
}

public class ErrorInternalServerExample : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples()
    {
        return new ErrorResponse
        {
            Code = 500,
            Detail = "internal_server_error",
            Attr = null
        };
    }
}
