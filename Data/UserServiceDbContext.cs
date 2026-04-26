using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using user_service.Models;

namespace user_service.Data;

public sealed class UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Preference> Preferences => Set<Preference>();
    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserAction> UserActions => Set<UserAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var stringListConverter = new ValueConverter<List<string>, string>(
            v => SerializeStringList(v),
            v => DeserializeStringList(v));

        var stringListComparer = new ValueComparer<List<string>>(
            (a, b) => AreListsEqual(a, b),
            list => GetListHashCode(list),
            list => CloneList(list));

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.Username).IsUnique();

            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Username).HasMaxLength(64);
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.Property(x => x.FirstName).HasMaxLength(128);
            entity.Property(x => x.LastName).HasMaxLength(128);
            entity.Property(x => x.Phone).HasMaxLength(32);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(64);
            entity.Property(x => x.Description).HasMaxLength(256);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(x => new { x.UserId, x.RoleId });

            entity
                .HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Preference>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FavoriteCategories)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);

            entity.Property(x => x.FavoriteArtists)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);

            entity.Property(x => x.PreferredCities)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);

            entity.Property(x => x.MinPrice).HasPrecision(18, 2);
            entity.Property(x => x.MaxPrice).HasPrecision(18, 2);
            entity.Property(x => x.PreferredEventTime).HasMaxLength(64);

            entity
                .HasOne(x => x.User)
                .WithOne(x => x.Preferences)
                .HasForeignKey<Preference>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId).IsUnique();
        });

        modelBuilder.Entity<NotificationSettings>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity
                .HasOne(x => x.User)
                .WithOne(x => x.NotificationSettings)
                .HasForeignKey<NotificationSettings>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(512);

            entity
                .HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<UserAction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity
                .HasOne(x => x.User)
                .WithMany(x => x.UserActions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.EventId);
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("5E8E5F7A-0BA9-44D9-A9CC-B2C34D1821AA"), Name = "USER", Description = "Default user role" },
            new Role { Id = Guid.Parse("67D786A4-86FA-4E8E-B038-451C0B06F0F0"), Name = "ADMIN", Description = "Administrator role" },
            new Role { Id = Guid.Parse("5A17334F-7431-4256-859A-9B2251EF7E22"), Name = "PREMIUMUSER", Description = "Premium user role" }
        );
    }

    private static string SerializeStringList(List<string>? value)
    {
        return JsonSerializer.Serialize(value ?? new List<string>());
    }

    private static List<string> DeserializeStringList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        return JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();
    }

    private static bool AreListsEqual(List<string>? a, List<string>? b)
    {
        return (a ?? new List<string>()).SequenceEqual(b ?? new List<string>());
    }

    private static int GetListHashCode(List<string>? list)
    {
        if (list is null)
        {
            return 0;
        }

        var hash = new HashCode();
        foreach (var item in list)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    private static List<string> CloneList(List<string>? list)
    {
        return list?.ToList() ?? new List<string>();
    }
}
