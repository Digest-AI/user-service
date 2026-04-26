namespace user_service.DTOs.User;

public sealed class PreferenceDto
{
    public IReadOnlyCollection<string> FavoriteCategories { get; set; } = [];
    public IReadOnlyCollection<string> FavoriteArtists { get; set; } = [];
    public IReadOnlyCollection<string> PreferredCities { get; set; } = [];
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? PreferredEventTime { get; set; }
    public int NotificationBeforeHours { get; set; }
}

public sealed class UpdatePreferenceRequest
{
    public List<string> FavoriteCategories { get; set; } = [];
    public List<string> FavoriteArtists { get; set; } = [];
    public List<string> PreferredCities { get; set; } = [];
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? PreferredEventTime { get; set; }
    public int NotificationBeforeHours { get; set; } = 24;
}
