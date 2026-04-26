namespace user_service.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "user-service";
    public string Audience { get; set; } = "digest-ai-clients";
    public string Key { get; set; } = "SUPER_SECRET_KEY_CHANGE_ME_12345678901234567890";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public int RefreshTokenRememberMeDays { get; set; } = 30;
}
