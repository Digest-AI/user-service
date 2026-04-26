namespace user_service.Models;

public sealed class Preference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public List<string> FavoriteCategories { get; set; } = [];
    public List<string> FavoriteArtists { get; set; } = [];
    public List<string> PreferredCities { get; set; } = [];

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? PreferredEventTime { get; set; }
    public int NotificationBeforeHours { get; set; } = 24;

    public User User { get; set; } = null!;
}
