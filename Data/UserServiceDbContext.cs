using Microsoft.EntityFrameworkCore;
using user_service.Models;

namespace user_service.Data;

public sealed class UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PendingRegistration> PendingRegistrations => Set<PendingRegistration>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();
    public DbSet<Chain> Chains => Set<Chain>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.PublicId).IsUnique();

            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.Property(x => x.Name).HasMaxLength(128);
            entity.Property(x => x.Surname).HasMaxLength(128);
            entity.Property(x => x.Gender).HasMaxLength(32);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PendingRegistration>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.Code);
            entity.HasIndex(x => x.ExpiresAt);

            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.Property(x => x.Name).HasMaxLength(128);
            entity.Property(x => x.Surname).HasMaxLength(128);
            entity.Property(x => x.Gender).HasMaxLength(32);
            entity.Property(x => x.Code).HasMaxLength(6);
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

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<VerificationCode>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(6);
            entity.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(32);

            entity.HasOne(x => x.User)
                .WithMany(x => x.VerificationCodes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Code);
            entity.HasIndex(x => x.ExpiresAt);
        });

        modelBuilder.Entity<Chain>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Chains)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(512);

            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Chain)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.ChainId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.ChainId);
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("5E8E5F7A-0BA9-44D9-A9CC-B2C34D1821AA"), Name = "USER", Description = "Default user role" },
            new Role { Id = Guid.Parse("67D786A4-86FA-4E8E-B038-451C0B06F0F0"), Name = "ADMIN", Description = "Administrator role" },
            new Role { Id = Guid.Parse("5A17334F-7431-4256-859A-9B2251EF7E22"), Name = "PREMIUMUSER", Description = "Premium user role" }
        );
    }
}
