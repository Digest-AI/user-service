namespace user_service.DTOs.Common;

/// <summary>
/// Standard error response for API endpoints.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Error code identifier (e.g., "invalid_email_format").
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Field name or attribute that the error relates to. Nullable.
    /// </summary>
    public string? Attr { get; set; }
}
