using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Infrastructure.Entities;

namespace TechnoDating.Api.Infrastructure;

public class TechnoDatingDbContext(DbContextOptions<TechnoDatingDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Festival> Festivals => Set<Festival>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserFestivalAttendance> Attendances => Set<UserFestivalAttendance>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<UserTopArtist> UserTopArtists => Set<UserTopArtist>();
    public DbSet<FestivalHeadlineArtist> FestivalHeadlineArtists => Set<FestivalHeadlineArtist>();
    public DbSet<Photo> Photos => Set<Photo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.DisplayName).HasMaxLength(100);
            b.Property(u => u.Gender).HasMaxLength(32);
            b.Property(u => u.Bio).HasMaxLength(2000);
            b.Property(u => u.City).HasMaxLength(100);
            b.Property(u => u.Location).HasColumnType("geography(Point, 4326)");
            b.Ignore(u => u.IsProfileComplete);
        });

        modelBuilder.Entity<Festival>(b =>
        {
            b.HasKey(f => f.Id);
            b.Property(f => f.Name).IsRequired().HasMaxLength(200);
            b.Property(f => f.City).IsRequired().HasMaxLength(100);
            b.Property(f => f.Venue).IsRequired().HasMaxLength(200);
            b.Property(f => f.Location).HasColumnType("geography(Point, 4326)");
        });

        modelBuilder.Entity<Match>(b =>
        {
            b.HasKey(m => m.Id);
            b.HasIndex(m => new { m.UserAId, m.UserBId }).IsUnique();
            b.HasOne(m => m.UserA).WithMany().HasForeignKey(m => m.UserAId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(m => m.UserB).WithMany().HasForeignKey(m => m.UserBId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OtpChallenge>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(32);
            b.Property(o => o.CodeHash).IsRequired().HasMaxLength(512);
            b.HasIndex(o => new { o.PhoneNumber, o.ExpiresAt });
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.TokenHash).IsRequired().HasMaxLength(512);
            b.HasIndex(r => r.TokenHash).IsUnique();
            b.HasIndex(r => new { r.UserId, r.ExpiresAt });
            b.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserFestivalAttendance>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            b.HasIndex(a => new { a.UserId, a.FestivalId }).IsUnique();
            b.HasIndex(a => a.FestivalId);
            b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.Festival).WithMany().HasForeignKey(a => a.FestivalId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Artist>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Name).IsRequired().HasMaxLength(200);
            b.Property(a => a.Slug).IsRequired().HasMaxLength(200);
            b.Property(a => a.Genre).HasMaxLength(64);
            b.HasIndex(a => a.Slug).IsUnique();
            b.HasIndex(a => a.Name);
        });

        modelBuilder.Entity<UserTopArtist>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.UserId, x.ArtistId }).IsUnique();
            b.HasIndex(x => x.UserId);
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Artist).WithMany().HasForeignKey(x => x.ArtistId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FestivalHeadlineArtist>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.FestivalId, x.ArtistId }).IsUnique();
            b.HasIndex(x => x.FestivalId);
            b.HasOne(x => x.Festival).WithMany().HasForeignKey(x => x.FestivalId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Artist).WithMany().HasForeignKey(x => x.ArtistId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Photo>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.StorageKey).IsRequired().HasMaxLength(512);
            b.Property(p => p.ContentType).IsRequired().HasMaxLength(64);
            b.Property(p => p.ModerationStatus).IsRequired().HasMaxLength(32);
            b.HasIndex(p => new { p.UserId, p.Ordinal }).IsUnique();
            b.HasIndex(p => new { p.UserId, p.IsPrimary })
                .IsUnique()
                .HasFilter("\"IsPrimary\" = true");
            b.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
