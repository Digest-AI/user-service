namespace user_service.Options;

public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";

    public string ApiKey { get; set; } = "__SET_IN_USER_SECRETS__";
    public string FromEmail { get; set; } = "__SET_IN_USER_SECRETS__";
    public string? FromName { get; set; } = "Digest.AI";
}
